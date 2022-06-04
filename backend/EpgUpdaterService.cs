using Microsoft.Extensions.Options;

namespace backend;

public class EpgUpdaterService : BackgroundService
{
  private readonly IOptions<AppSettings> _settings;
  private readonly TvhApi _tvhApi;
  public IReadOnlyList<Channel> CurrentEpg { get; set; } = Array.Empty<Channel>();

  public EpgUpdaterService(IOptions<AppSettings> settings, TvhApi tvhApi)
  {
    _settings = settings;
    _tvhApi = tvhApi;
  }
  
  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {
    return Task.Run(async () =>
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        this.CurrentEpg = await FetchEpg();
        await Task.Delay(30000, stoppingToken);
      }
    }, stoppingToken);
  }

  private async Task<IReadOnlyList<Channel>> FetchEpg()
  {
    var result = new List<Channel>();
    
    var tvhChannels = await _tvhApi.GetChannels();
    
    foreach (var tvhChannel in tvhChannels)
    {
      var tvhChannelEpg = await _tvhApi.GetEpgForChannel(tvhChannel.Uuid);
    
      var channel = new Channel()
      {
        Name = tvhChannel.Name,
        Uuid = tvhChannel.Uuid,
        Number = tvhChannel.Number,
        IconUrl = tvhChannel.IconPublicUrl,
        EpgEntries = tvhChannelEpg.Select(e => new EpgEvent()
        {
          Description = e.Description,
          Start = DateTimeOffset.FromUnixTimeSeconds(e.Start).DateTime,
          Stop = DateTimeOffset.FromUnixTimeSeconds(e.Stop).DateTime,
          Title =e.Title,
          EventId = e.EventId,
          IsScheduled = e.DvrState == TvhDvrState.Scheduled,
        }).ToArray()
      };
      
      result.Add(channel);
    }

    return result;
  }
}