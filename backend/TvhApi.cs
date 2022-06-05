using Flurl;
using Flurl.Http;
using Newtonsoft.Json;

namespace backend;

public class TvhApi
{
  private readonly string _baseUrl;

  public TvhApi(string baseUrl, string user, string pass)
  {
    _baseUrl = baseUrl.AppendPathSegment("/api");

    FlurlHttp.ConfigureClient(baseUrl, s =>
    {
      s.WithBasicAuth(user, pass);
    });
  }

  public async Task<IEnumerable<TvhEpgEvent>> GetFullEpg()
  {
    var json = await _baseUrl
      .AppendPathSegment("/epg/events/grid")
      .SetQueryParam("limit", int.MaxValue)
      .GetJsonAsync<TvhEpgEventResponse>();

    return json.Entries;
  }
  
  public async Task<TvhEpgEvent> RefreshEpgEvent(ulong epgEventId)
  {
    var json = await _baseUrl.AppendPathSegment("epg/events/load")
      .SetQueryParam("eventId", epgEventId)
      .GetAsync()
      .ReceiveJson<TvhEpgEventResponse>();

    return json.Entries.First();
  }
  
  public async Task<IEnumerable<TvhEpgEvent>> GetEpgForChannel(string channelUuid)
  {
    var json = await _baseUrl
      .AppendPathSegment("/epg/events/grid")
      .SetQueryParam("channel", channelUuid)
      .SetQueryParam("limit", int.MaxValue)
      .GetJsonAsync<TvhEpgEventResponse>();
  
    return json.Entries;
  }
  
  public async Task<IEnumerable<TvhEpgGenre>> GetEpgGenres()
  {
    var json = await _baseUrl
      .AppendPathSegment("/epg/content_type/list")
      .SetQueryParam("full", 1)
      .GetJsonAsync<TvhEpgGenreResponse>();

    return json.Entries;
  }
  
  public async Task<byte[]> GetChannelIcon(int id)
  {
    try
    {
      return await _baseUrl.RemovePathSegment()
        .AppendPathSegments("imagecache", id)
        .GetBytesAsync();
    }
    catch (Exception)
    {
      return File.ReadAllBytes("Default.png");
    }
  }
  
  public async Task<IEnumerable<TvhChannel>> GetChannels()
  {
    var json = await _baseUrl
      .AppendPathSegment("/channel/grid")
      .GetJsonAsync<TvhChannelResponse>();

    return json.Entries;
  }
  
  public async Task<string[]?> RecordEpgEvent(ulong epgEventId)
  {
    var json = await _baseUrl.AppendPathSegment("/dvr/config/grid")
      .GetJsonAsync<TvhDvrConfigResponse>();

    var primaryDvrConfig = json.Entries.Where(e => e.IsEnabled).OrderBy(e => e.Pri).First();

    var res = await _baseUrl.AppendPathSegment("/dvr/entry/create_by_event")
      .SetQueryParam("config_uuid", primaryDvrConfig.Uuid)
      .SetQueryParam("event_id", epgEventId)
      .GetJsonAsync<TvhDvrCreateResponse>();

    return res.Uuid;
  }
  
  public async Task DeleteTimerEpgEvent(ulong epgEventId)
  {
    var epgEvent = await this.RefreshEpgEvent(epgEventId);

    var res = await _baseUrl.AppendPathSegment("/dvr/entry/cancel")
      .SetQueryParam("uuid", epgEvent.DvrUuid)
      .GetAsync();
  }
}

public class TvhDvrCreateResponse
{
  public string[]? Uuid { get; set; }
}

public class TvhDvrConfigResponse
{
  public ulong Total { get; set; }
  public IEnumerable<TvhDvrConfig> Entries { get; set; } = null!;
}

public class TvhChannelResponse
{
  public ulong Total { get; set; }
  public IEnumerable<TvhChannel> Entries { get; set; } = null!;
}

public class TvhEpgGenreResponse
{
  public ulong Total { get; set; }
  public IEnumerable<TvhEpgGenre> Entries { get; set; } = null!;
}

public class TvhEpgEventResponse
{
  public ulong TotalCount { get; set; }
  public IEnumerable<TvhEpgEvent> Entries { get; set; } = null!;
}

public record TvhEpgGenre
{
  [JsonProperty("key")] public int Id { get; set; }
  [JsonProperty("val")] public string Name { get; set; } = null!;
}

public record TvhChannel
{
  public string Uuid { get; set; } = null!;
  [JsonProperty("enabled")] public bool IsEnabled { get; set; }
  public string Name { get; set; } = null!;
  public int Number { get; set; }
  public string Icon { get; set; } = null!;
  [JsonProperty("icon_public_url")] public string IconPublicUrl { get; set; } = null!;
}

public record TvhEpgEvent
{
  public long EventId { get; set; }
  public string ChannelName { get; set; } = null!;
  public string ChannelUuid { get; set; } = null!;
  public long ChannelNumber { get; set; }
  public string ChannelIcon { get; set; } = null!;
  public long Start { get; set; }
  public long Stop { get; set; }
  public long NextEventId { get; set; }
  public int[] Genre { get; set; } = Array.Empty<int>();
  public string Title { get; set; } = null!;
  public string Subtitle { get; set; } = null!;
  public string Description { get; set; } = null!;
  public long Widescreen { get; set; }
  [JsonProperty("hd")] public bool IsHd { get; set; }
  public string? DvrUuid { get; set; }
  public TvhDvrState? DvrState { get; set; }
}

public enum TvhDvrState
{
  Scheduled,
  Completed,
  Recording,
  CompletedError,
  CompletedWarning,
  CompletedRerecord
}

public class TvhDvrConfig
{
  public string Uuid { get; set; } = null!;
  [JsonProperty("enabled")] public bool IsEnabled { get; set; }
  public string Name { get; set; } = null!;
  public string Profile { get; set; } = null!;
  public int Pri { get; set; }
  [JsonProperty("pre-extra-time")] public int PreExtraTime { get; set; }
  [JsonProperty("post-extra-time")] public int PostExtraTime { get; set; }
  public string Storage { get; set; } = null!;
  [JsonProperty("storage-mfree")] public uint StorageMfree { get; set; }
  [JsonProperty("storage-mused")] public uint StorageMused { get; set; }
}

