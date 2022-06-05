<script>
  import { tick } from "svelte";
  import { add, sub, format } from "date-fns";
  import de from "date-fns/locale/de";
  import { fade, fly } from "svelte/transition";

  // import { dev } from "$app/env";

  // if (dev) {
  //   //do in dev mode
  // }

  let epg = [];
  let loading = true;
  let scale = 10;

  let url = import.meta.env.PROD ? "/" : "http://localhost:4500/";

  async function fetchEpg() {
    console.time();
    let res = await fetch(url + "epg");
    let json = await res.json();

    json.forEach((channel) => {
      channel.epgEntries.forEach((epgEntry) => {
        epgEntry.start = new Date(epgEntry.start);
        epgEntry.stop = new Date(epgEntry.stop);
      });
    });

    epg = json;

    await tick();

    loading = false;

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

  let selectedEpgEntry = null;

  async function setTimer(epgEntry) {
    if (epgEntry.isScheduled) {
      let res = await fetch(url + "record/" + epgEntry.eventId, {
        method: "DELETE",
      });

      if (await res.json()) selectedEpgEntry.isScheduled = false;
    } else {
      let res = await fetch(url + "record/" + epgEntry.eventId);

      if (await res.json()) selectedEpgEntry.isScheduled = true;
    }
  }
</script>

<svelte:window on:resize={resize} />

<!-- MODAL POPUP -->

<div
  class:hidden={!selectedEpgEntry}
  class="fixed z-50 inset-0 bg-gray-900 bg-opacity-50"
>
  <div class="absolute inset-5 rounded border bg-gray-100 p-5 border-gray-900 ">
    <div class="font-bold text-2xl">
      {selectedEpgEntry?.title}
      {#if selectedEpgEntry?.isScheduled}
        🔴
      {/if}
    </div>
    <div class="text-sm mb-2">
      ({selectedEpgEntry?.startString}
      -
      {selectedEpgEntry?.stopString})
    </div>
    <div class="mt-5">
      {selectedEpgEntry?.description ?? ""}
    </div>
    <div class="mt-10">
      <button
        on:click={() => setTimer(selectedEpgEntry)}
        class="rounded bg-red-500 w-32 p-4 font-bold text-white"
      >
        {selectedEpgEntry?.isScheduled ? "Löschen" : "Aufnahme"}
      </button>
      <button
        on:click={() => (selectedEpgEntry = null)}
        class="rounded bg-blue-500 w-32 p-4 font-bold text-white"
        >Schließen</button
      >
    </div>
  </div>
</div>

<div
  class:hidden={!loading}
  class="fixed m-auto left-0 right-0 top-0 bottom-0 h-1 w-1"
>
  <svg
    xmlns="http://www.w3.org/2000/svg"
    class="h-12 w-12 animate-spin"
    fill="none"
    viewBox="0 0 24 24"
    stroke="currentColor"
    stroke-width="2"
  >
    <path
      stroke-linecap="round"
      stroke-linejoin="round"
      d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"
    />
    <path
      stroke-linecap="round"
      stroke-linejoin="round"
      d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
    />
  </svg>
</div>

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

<div class="w-max bg-gray-900 select-none">
  <div
    style="height: calc({epg.length} * 6.25rem - 8px);"
    class="absolute pointer-events-none top-12 left-0 z-20  border-r-2 border-orange-500"
  >
    <div class="w-96 h-full opacity-30 bg-gray-500" />
  </div>

  {#each epg as channel}
    <div class="flex">
      <div
        style="min-width: 100px;"
        class="bg-gray-800 z-30 sticky left-0 p-2 w-24 h-24 flex justify-center items-center"
      >
        <img class="pointer-events-none" src="{url}{channel.iconUrl}" alt="" />
      </div>
      <div
        style="margin-left: {calcStartOffset(channel, scale)}px;"
        class="flex"
      >
        {#each channel.epgEntries as epgEntry}
          <div
            on:click={() => (selectedEpgEntry = epgEntry)}
            style="min-width: 0px; width: {calcSecondsToPixel(
              epgEntry,
              scale
            )}px"
            class="mb-1 cursor-pointer h-24 {getBackground(
              epgEntry
            )} overflow-hidden whitespace-nowrap"
          >
            <div class="text-gray-900 border-l-4 h-full p-2 border-gray-900">
              <div class="font-bold text-xl">
                {epgEntry.title}
                {#if epgEntry.isScheduled}
                  🔴
                {/if}
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
