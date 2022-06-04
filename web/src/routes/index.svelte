<script>
  let epg = [];

  let scale = 10;

  $: {
    console.log(scale);
  }

  async function fetchEpg() {
    let res = await fetch("http://localhost:4500/epg");
    let json = await res.json();

    var set = new Set();

    json.forEach((channel) => {
      channel.epgEntries.forEach((epgEntry) => {
        epgEntry.start = new Date(epgEntry.start);
        epgEntry.stop = new Date(epgEntry.stop);
        set.add(epgEntry.genre);
      });
    });

    console.log(set);

    epg = json;
  }

  fetchEpg();

  function calcSecondsToPixel(epgEntry) {
    let durationMs = epgEntry.stop - epgEntry.start;

    return durationMs / 1000 / scale;
  }

  function calcStartOffset(channel) {
    return (channel.epgEntries?.[0]?.start - new Date()) / 1000 / scale + 100;
  }

  //
  function getBackground(epgEntry) {
    var now = new Date();

    let genre = epgEntry.genre.toLowerCase();

    //todo: aktuelles dunkler
    if (epgEntry.start < now && now < epgEntry.stop) {
      return "bg-gray-300";
    } else if (genre.includes("news") || genre.includes("social")) {
      return "bg-blue-200";
    } else if (
      genre.includes("drama") ||
      genre.includes("detective") ||
      genre.includes("science") ||
      genre.includes("film")
    ) {
      return "bg-red-200";
    } else if (
      genre.includes("education") ||
      genre.includes("docu") ||
      genre.includes("nature")
    ) {
      return "bg-emerald-200";
    } else if (genre.includes("sports") || genre.includes("soccer")) {
      return "bg-pink-200";
    } else if (
      genre.includes("show") ||
      genre.includes("rock") ||
      genre.includes("arts") ||
      genre.includes("music")
    ) {
      return "bg-fuchsia-200";
    } else if (genre.includes("cooking")) {
      return "bg-violet-200";
    } else if (genre.includes("news")) {
      return "bg-blue-200";
    } else if (genre.includes("news")) {
      return "bg-blue-200";
    } else if (genre.includes("news")) {
      return "bg-blue-200";
    } else if (genre.includes("news")) {
      return "bg-blue-200";
    } else if (genre.includes("news")) {
      return "bg-blue-200";
    } else {
      return "bg-gray-200";
    }
  }

  function resize(e) {
    scale = window.innerWidth / 100;
  }
</script>

<svelte:window on:resize={resize} />

<div class="w-max">
  {#each epg as channel}
    <div class="flex">
      <div
        style="min-width: 100px;"
        class="bg-gray-800 z-30 sticky left-0 p-2 w-24 h-24 flex justify-center items-center"
      >
        <img src="http://localhost:4500/{channel.iconUrl}" alt="" />
      </div>
      <div style="margin-left: {calcStartOffset(channel)}px;" class="flex">
        {#each channel.epgEntries as epgEntry}
          <div
            style="min-width: 0px; width: {calcSecondsToPixel(epgEntry)}px"
            class="{getBackground(
              epgEntry
            )} border border-gray-900 h-24 overflow-hidden whitespace-nowrap"
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
</div>
