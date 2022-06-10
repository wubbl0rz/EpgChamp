using System.ComponentModel;
using System.Reflection;
using backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Spectre.Console.Cli;

var cli = new CommandApp();

cli.Configure(config =>
{
  config.AddDelegate<StartSettings>("start", (cliCommand, cliSettings) =>
  {
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddCors();
    //builder.Configuration.AddJsonFile("config.json");
    builder.Services.Configure<AppSettings>(s =>
    {
      s.TvhUser = cliSettings.User;
      s.TvhPass = cliSettings.Pass;
      s.TvhUrl = cliSettings.Url;
      s.EpgNumberOfDays = 7;
    });

    builder.Services.AddSingleton<TvhApi>(p =>
    {
      var settings = p.GetRequiredService<IOptions<AppSettings>>().Value;
      return new TvhApi(settings.TvhUrl, settings.TvhUser, settings.TvhPass);
    });

    builder.Services.AddSingleton<EpgUpdaterService>();
    builder.Services.AddHostedService(p => p.GetRequiredService<EpgUpdaterService>());
    builder.Services.AddTransient(p => p.GetRequiredService<EpgUpdaterService>().CurrentEpg);

    var app = builder.Build();

    var manifestEmbeddedProvider =
      new ManifestEmbeddedFileProvider(typeof(Program).Assembly, "wwwroot");

    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
      app.UseSwaggerUI();
      app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
    }

    app.UseFileServer(new FileServerOptions()
    {
      FileProvider = manifestEmbeddedProvider,
    });

    app.MapGet("/epg", ([FromServices] EpgCache epgCache) => epgCache.EpgWeek);
    
    app.MapGet("/imagecache/{id}", async ([FromServices] TvhApi api, int id)
      => Results.File(await api.GetChannelIcon(id), "image/png"));

    app.MapGet("/record/{id}", async ([FromServices] TvhApi api, [FromServices] EpgCache epgCache, long id) =>
    {
      await api.AddTimerForEpgEvent(id);

      epgCache.UpdateEpgEvent(id, entry => { entry.IsScheduled = true; });
    });

    app.MapDelete("/record/{id}", async ([FromServices] TvhApi api, [FromServices] EpgCache epgCache, long id) =>
    {
      await api.DeleteTimerForEpgEvent(id);

      epgCache.UpdateEpgEvent(id, entry => { entry.IsScheduled = false; });
    });

    app.Run($"http://*:{cliSettings.Port}");

    return 0;
  });
});

return cli.Run(args);

public class StartSettings : CommandSettings
{
  [Description("Listen port for server. Default: 5000")]
  [CommandOption("-l|--port")]
  public UInt16 Port { get; init; } = 5000;

  [Description("User fot Tvheadend with API access.")]
  [CommandOption("-u|--user")]
  public string User { get; init; } = null!;

  [Description("Password for Tvheadend user.")]
  [CommandOption("-p|--pass")]
  public string Pass { get; init; } = null!;

  [Description("Url to Tvheadend installation. E.g. http://my.domain.or.ip:9981. Default: http://localhost:9981")]
  [CommandOption("-t|--tvh-url")]
  public string Url { get; init; } = "http://localhost:9981";
}

public record Channel
{
  public string Uuid { get; set; } = "";
  public string Name { get; set; } = "";
  public int Number { get; set; }
  public string IconUrl { get; set; } = "";
  public IEnumerable<EpgEntry> EpgEntries { get; set; } = Array.Empty<EpgEntry>();
}

public record EpgEntry
{
  public long EventId { get; set; }
  public DateTime Start { get; set; }
  public DateTime Stop { get; set; }
  public string StartString { get; set; } = "";
  public string StopString { get; set; } = "";
  public string Description { get; set; } = "";
  public string Title { get; set; } = "";
  public bool IsScheduled { get; set; }
  public string Genre { get; set; } = "";
}

public class AppSettings
{
  public int EpgNumberOfDays { get; set; } = 7;
  public string TvhUser { get; set; } = null!;
  public string TvhPass { get; set; } = null!;
  public string TvhUrl { get; set; } = null!;
}