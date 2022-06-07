using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace backend;

public class EpgCache
{
  private readonly ConcurrentDictionary<string, Channel> _epgData = new();

  public IEnumerable<Channel> Epg => _epgData.Select(c => c.Value).OrderBy(c => c.Number);

  public EpgCache(IEnumerable<Channel>? epgData = null)
  {
    if (epgData == null) 
      return;
    
    foreach (var channel in epgData)
    {
      this.SetChannel(channel);
    }
  }

  public void SetChannel(Channel channel)
  {
    _epgData[channel.Uuid] = channel;
  }

  public void UpdateEpgEvent(long eventId, Action<EpgEntry> cb)
  {
    var epgEntry = _epgData
      .SelectMany(c => c.Value.EpgEntries)
      .First(e => e.EventId == eventId);
    
    cb?.Invoke(epgEntry);
  }
}

public class EpgUpdaterService : BackgroundService
{
  private readonly IOptions<AppSettings> _settings;
  private readonly TvhApi _tvhApi;
  public EpgCache CurrentEpg { get; set; } = new();
  
  public EpgUpdaterService(IOptions<AppSettings> settings, TvhApi tvhApi)
  {
    _settings = settings;
    _tvhApi = tvhApi;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      await FetchEpg();
      AnsiConsole.MarkupLine("[blue]EPG REFRESHED[/]");
      await Task.Delay(30000, stoppingToken);
    }
  }

  private async Task FetchEpg()
  {
    var tvhChannels = await _tvhApi.GetChannels();

    foreach (var tvhChannel in tvhChannels)
    {
      var genres = await _tvhApi.GetEpgGenres();
      
      var tvhChannelEpg = await _tvhApi.GetEpgForChannel(tvhChannel.Uuid);

      var epgEntries = tvhChannelEpg.Select(e =>
        {
          var start = DateTimeOffset.FromUnixTimeSeconds(e.Start).LocalDateTime;
          var stop = DateTimeOffset.FromUnixTimeSeconds(e.Stop).LocalDateTime;
          var genre = genres.First(g => g.Id == e.Genre.FirstOrDefault(0)).Name;

          return new EpgEntry
          {
            Description = e.Description,
            Start = start,
            Stop = stop,
            StartString = start.ToString("t", new CultureInfo("de-DE")),
            StopString = stop.ToString("t", new CultureInfo("de-DE")),
            Title = e.Title,
            EventId = e.EventId,
            IsScheduled = e.DvrState == TvhDvrState.Scheduled,
            Genre = genre
          };
        })
        .Where(e => e.Start < DateTime.Now.AddDays(7))
        .ToArray();

      for (int i = 1; i < epgEntries.Length; i++)
      {
        var last = epgEntries[i - 1];
        var current = epgEntries[i];

        last.Stop = current.Start;
      }

      var channel = new Channel()
      {
        Name = tvhChannel.Name,
        Uuid = tvhChannel.Uuid,
        Number = tvhChannel.Number,
        IconUrl = tvhChannel.IconPublicUrl,
        EpgEntries = epgEntries
      };

      this.CurrentEpg.SetChannel(channel);
    }
  }
}