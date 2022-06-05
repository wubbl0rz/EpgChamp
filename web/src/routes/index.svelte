<script>
  import { tick } from "svelte";
  import { add, sub, format } from "date-fns";
  import de from "date-fns/locale/de";

  let epg = [];

  let scale = 10;

  async function fetchEpg() {
    console.time();
    let res = await fetch("http://localhost:4500/epg");
    let json = await res.json();

    json.forEach((channel) => {
      channel.epgEntries.forEach((epgEntry) => {
        epgEntry.start = new Date(epgEntry.start);
        epgEntry.stop = new Date(epgEntry.stop);
      });
    });

    epg = json;

    await tick();

    console.timeEnd();
  }

  fetchEpg();

  function calcSecondsToPixel(epgEntry) {
    let durationMs = epgEntry.stop - epgEntry.start;

    return durationMs / 1000 / scale;
  }

  function calcStartOffset(channel, scale) {
    return (channel.epgEntries?.[0]?.start - new Date()) / 1000 / scale;
  }

  function getBackground(epgEntry) {
    var now = new Date();

    let genre = epgEntry.genre.toLowerCase();

    // if (epgEntry.start < now && now < epgEntry.stop) {
    //   return "bg-gray-300";
    // }

    if (genre.includes("news") || genre.includes("social")) {
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
    //scale = 10000 / window.innerWidth;
  }

  let startNow = new Date();
  startNow.setHours(0);
  startNow.setMinutes(0);
</script>

<svelte:window on:resize={resize} />

<div style="width: 60480px;" class="overflow-hidden">
  <div
    class="flex text-gray-900"
    style="margin-left: -{(new Date() - startNow) / 10000}px;"
  >
    {#each Array(8) as _, y}
      <div class="flex bg-gradient-to-r h-12 w-12 time" style="width: 8640px;">
        {#each Array(24) as _, i}
          <div>
            <div style="width: 360px;">
              <div class="p-2 font-bold text-lg">
                {format(
                  add(startNow, { days: y, minutes: i * 60 }),
                  "EEEE, d. LLLL HH:mm",
                  {
                    locale: de,
                  }
                )}
              </div>
            </div>
          </div>
        {/each}
      </div>
    {/each}
  </div>
</div>

<div class="w-max bg-gray-900">
  <div
    style="height: calc({epg.length} * 6.25rem - 8px);"
    class="absolute top-12 left-0 z-20  border-r-2 border-orange-500"
  >
    <div class="w-96 h-full opacity-30 bg-gray-500" />
  </div>

  {#each epg as channel}
    <div class="flex">
      <div
        style="min-width: 100px;"
        class="bg-gray-800 z-30 sticky left-0 p-2 w-24 h-24 flex justify-center items-center"
      >
        <img src="http://localhost:4500/{channel.iconUrl}" alt="" />
      </div>
      <div
        style="margin-left: {calcStartOffset(channel, scale)}px;"
        class="flex"
      >
        {#each channel.epgEntries as epgEntry}
          <div
            style="min-width: 0px; width: {calcSecondsToPixel(
              epgEntry,
              scale
            )}px"
            class="mb-1 h-24 {getBackground(
              epgEntry
            )} overflow-hidden whitespace-nowrap"
          >
            <div class="text-gray-900 border-l-4 h-full p-2 border-gray-900">
              <div class="font-bold text-xl">
                {epgEntry.title}
              </div>
              <div class="text-lg">
                {epgEntry.startString}
              </div>
              <div class="text-lg">
                {epgEntry.stopString}
              </div>
            </div>
          </div>
        {/each}
      </div>
    </div>
  {/each}
</div>

<style>
  .time {
    background: linear-gradient(
      to right,
      #818cf8 10%,
      #facc15 33%,
      #facc15 66%,
      #818cf8 90%
    );
  }
</style>
