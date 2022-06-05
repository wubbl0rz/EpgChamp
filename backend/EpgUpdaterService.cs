using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace backend;

public class EpgUpdaterService : BackgroundService
{
  private readonly IOptions<AppSettings> _settings;
  private readonly TvhApi _tvhApi;
  public IReadOnlyList<Channel> CurrentEpg { get; set; } = Array.Empty<Channel>();

  private ManualResetEventSlim _resetEvent = new();

  public EpgUpdaterService(IOptions<AppSettings> settings, TvhApi tvhApi)
  {
    _settings = settings;
    _tvhApi = tvhApi;
  }

  public void TriggerRefresh()
  {
    // refresh 1 channel only ?
    _resetEvent.Set();
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      _resetEvent.Reset();
      await FetchEpg();
      AnsiConsole.MarkupLine("[blue]EPG REFRESH[/]");
      _resetEvent.Wait(30000, stoppingToken);
    }
  }

  private async Task FetchEpg()
  {
    var result = new List<Channel>();

    var tvhChannels = await _tvhApi.GetChannels();

    tvhChannels = tvhChannels.OrderBy(c => c.Number);

    foreach (var tvhChannel in tvhChannels)
    {
      var tvhChannelEpg = await _tvhApi.GetEpgForChannel(tvhChannel.Uuid);

      var genres = await _tvhApi.GetEpgGenres();

      var epgEntries = tvhChannelEpg.Select(e =>
      {
        var start = DateTimeOffset.FromUnixTimeSeconds(e.Start).LocalDateTime;
        var stop = DateTimeOffset.FromUnixTimeSeconds(e.Stop).LocalDateTime;
        var genre = genres.First(g => g.Id == e.Genre.FirstOrDefault(0)).Name;

        return new EpgEvent()
        {
          Description = e.Description,
          Start = start,
          Stop = stop,
          StartString = start.ToString("t", new CultureInfo("de-DE")),
          StopString = stop.ToString("t", new CultureInfo("de-DE")),
          Title = e.Title,
          EventId = e.EventId,
          DvrUuid = e.DvrUuid,
          IsScheduled = e.DvrState == TvhDvrState.Scheduled,
          Genre = genre
        };
      }).Where(e => e.Start < DateTime.Now.AddDays(7)).ToList();
      
      // keep 1 day history
      //var oldChannel = this.CurrentEpg.FirstOrDefault(c => c.Uuid == tvhChannel.Uuid);
      // if (oldChannel != null)
      // {
      //   epgEntries = epgEntries
      //     .Concat(oldChannel.EpgEntries).DistinctBy(e => e.EventId)
      //     .Where(e => e.Start > DateTime.Now.AddDays(-1))
      //     .Where(e => e.Start < DateTime.Now.AddDays(7)).ToList();
      // }

      EpgEvent? lastEpgEntry = null;
      
      // fix gap between entries
      foreach (var epgEntry in epgEntries)
      {
        if (lastEpgEntry != null && lastEpgEntry.Stop < epgEntry.Start)
        {
          lastEpgEntry.Stop = epgEntry.Start;
        }
        
        lastEpgEntry = epgEntry;
      }

      var channel = new Channel()
      {
        Name = tvhChannel.Name,
        Uuid = tvhChannel.Uuid,
        Number = tvhChannel.Number,
        IconUrl = tvhChannel.IconPublicUrl,
        EpgEntries = epgEntries
      };

      result.Add(channel);
    }

    this.CurrentEpg = result;
  }
}