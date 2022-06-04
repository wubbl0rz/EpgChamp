using backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
builder.Configuration.AddJsonFile("config.json");
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddSingleton<TvhApi>(p =>
{
  var settings = p.GetRequiredService<IOptions<AppSettings>>().Value;
  return new TvhApi(settings.TvhUrl, settings.TvhUser, settings.TvhPass);
});

builder.Services.AddSingleton<EpgUpdaterService>();
builder.Services.AddHostedService(p => p.GetRequiredService<EpgUpdaterService>());

var app = builder.Build();

app.UseFileServer();

if (app.Environment.IsDevelopment())
{
  app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
}

app.MapGet("/epg", ([FromServices]EpgUpdaterService epg) => epg.CurrentEpg);

app.Run();

public class Channel
{
  public string Uuid { get; set; } = "";
  public string Name { get; set; } = "";
  public int Number { get; set; }
  public string IconUrl { get; set; } = "";
  public EpgEvent[] EpgEntries { get; set; } = Array.Empty<EpgEvent>();
}

public class EpgEvent
{
  public long EventId { get; set; }
  public DateTime Start { get; set; }
  public DateTime Stop { get; set; }
  public string Description { get; set; } = "";
  public string Title { get; set; } = "";
  public bool IsScheduled { get; set; }
}

public class AppSettings
{
  public int EpgNumberOfDays { get; set; } = 7;
  public string TvhUser { get; set; } = null!;
  public string TvhPass { get; set; } = null!;
  public string TvhUrl { get; set; } = null!;
}

