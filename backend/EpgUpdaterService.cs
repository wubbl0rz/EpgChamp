using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace backend;

public class EpgCache
{
  private readonly ConcurrentDictionary<string, Channel> _epgData = new();

  public IEnumerable<Channel> EpgWeek => this.GetEpgUntil(DateTime.Now.AddDays(7));
  public IEnumerable<Channel> EpgNow => this.GetEpgUntil(DateTime.Now.AddDays(1));

  public IEnumerable<Channel> GetEpgUntil(DateTime end)
  {
    var channels = _epgData.Select(c =>
    {
      var channel = c.Value;
      
      var newChannel = channel with
      {
        EpgEntries = channel.EpgEntries.Where(e => e.Start < end)
      };
      
      return newChannel;
    }).ToArray();

    var max = channels.Max(c => c.EpgEntries.Max(e => e.Stop));

    foreach (var channel in channels)
    {
      var lastEpgEntry = channel.EpgEntries.Last();
      var epgEntries = channel.EpgEntries.ToList();
      
      var start = lastEpgEntry.Stop;
      var stop = max;
      
      if (lastEpgEntry.Stop < max)
      {
        epgEntries.Add(new EpgEntry
        {
          Start = start,
          Stop = stop,
          StartString = start.ToString("t", new CultureInfo("de-DE")),
          StopString = stop.ToString("t", new CultureInfo("de-DE")),
        });

        channel.EpgEntries = epgEntries;
      }
    }

    return channels.OrderBy(c => c.Number);
  }

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
          IsScheduled = e.DvrState == TvhDvrState.Scheduled || e.DvrState == TvhDvrState.Recording,
          Genre = genre
        };
      }).ToList();

      for (int i = 1; i < epgEntries.Count; i++)
      {
        var last = epgEntries[i - 1];
        var current = epgEntries[i];

        last.Stop = current.Start;
      }

      if (epgEntries.Count == 0)
      {
        
        var start = DateTime.Now.AddHours(-1).ToLocalTime();
        var stop = DateTime.Now.AddDays(7).ToLocalTime();
        
        epgEntries.Add(new EpgEntry
        {
          Start = start,
          Stop = stop,
          StartString = start.ToString("t", new CultureInfo("de-DE")),
          StopString = stop.ToString("t", new CultureInfo("de-DE")),
        });
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