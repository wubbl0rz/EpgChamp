using backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
  app.UseSwagger();
  app.UseSwaggerUI();
  app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
}

app.MapGet("/epg", ([FromServices]EpgUpdaterService epg) => epg.CurrentEpg);

app.MapGet("/imagecache/{id}", async ([FromServices] TvhApi api, int id)
  => Results.File(await api.GetChannelIcon(id), "image/png"));

app.MapGet("/record/{id}", async ([FromServices] TvhApi api, 
  [FromServices]EpgUpdaterService epg, ulong id) =>
{
  await api.RecordEpgEvent(id);

  var res = await api.RefreshEpgEvent(id);
  
  epg.TriggerRefresh();

  return res.DvrState == TvhDvrState.Scheduled;
});

app.MapDelete("/record/{id}", async ([FromServices] TvhApi api, 
  [FromServices]EpgUpdaterService epg, ulong id) =>
{
  await api.DeleteTimerEpgEvent(id);
  
  var res = await api.RefreshEpgEvent(id);
  
  epg.TriggerRefresh();

  return res.DvrState != TvhDvrState.Scheduled;
});

app.Run();

public class Channel
{
  public string Uuid { get; set; } = "";
  public string Name { get; set; } = "";
  public int Number { get; set; }
  public string IconUrl { get; set; } = "";
  public IEnumerable<EpgEvent> EpgEntries { get; set; } = Array.Empty<EpgEvent>();
}

public class EpgEvent
{
  public long EventId { get; set; }
  public DateTime Start { get; set; }
  public DateTime Stop { get; set; }
  public string StartString { get; set; } = "";
  public string StopString { get; set; } = ""; 
  public string Description { get; set; } = "";
  public string Title { get; set; } = "";
  public bool IsScheduled { get; set; }
  public string? DvrUuid { get; set; }
  public string Genre { get; set; } = "";
}

public class AppSettings
{
  public int EpgNumberOfDays { get; set; } = 7;
  public string TvhUser { get; set; } = null!;
  public string TvhPass { get; set; } = null!;
  public string TvhUrl { get; set; } = null!;
}

