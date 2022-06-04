<script>
  let epg = [];

  let scale = 5;

  $: {
    console.log(scale);
  }

  async function fetchEpg() {
    let res = await fetch("http://localhost:4500/epg");
    let json = await res.json();

    json.forEach((channel) => {
      channel.epgEntries.forEach((epgEntry) => {
        epgEntry.start = new Date(epgEntry.start);
        epgEntry.stop = new Date(epgEntry.stop);
      });
    });

    epg = json;
  }

  fetchEpg();

  function calcSecondsToPixel(epgEntry) {
    let durationMs = epgEntry.stop - epgEntry.start;

    return durationMs / 1000 / scale;
  }

  function calcStartOffset(channel) {
    return (channel.epgEntries?.[0]?.start - new Date()) / 1000 / scale;
  }

  function getBackground(epgEntry) {}

  function resize(e) {
    scale = window.innerWidth / 100;
  }
</script>

<svelte:window on:resize={resize} />

{#each epg as channel}
  <div class="flex w-max">
    <div
      style="min-width: 100px;"
      class="bg-gray-800 sticky left-0 p-2 w-24 h-24 flex justify-center items-center"
    >
      <img src="http://localhost:4500/{channel.iconUrl}" alt="" />
    </div>
    <div style="margin-left: {calcStartOffset(channel)}px;" class="flex">
      {#each channel.epgEntries as epgEntry}
        <div
          style="min-width: 0px; width: {calcSecondsToPixel(epgEntry)}px"
          class="border border-gray-900 h-24 overflow-hidden bg-slate-200 whitespace-nowrap"
        >
          <div class="p-2">
            <div class="font-bold text-xl">
              {epgEntry.title}
            </div>
            <div>
              {epgEntry.start.toLocaleTimeString("de", {
                hour: "2-digit",
                minute: "2-digit",
              })}
            </div>
            <div>
              {epgEntry.stop.toLocaleTimeString("de", {
                hour: "2-digit",
                minute: "2-digit",
              })}
            </div>
          </div>
        </div>
      {/each}
    </div>
  </div>
{/each}
