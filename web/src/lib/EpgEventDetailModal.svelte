<script>
  import { fade, fly } from "svelte/transition";
  import Icon from "../lib/Icon.svelte";

  export let epgEntry;
  export let channel;

  let url = import.meta.env.PROD ? "/" : "http://localhost:4500/";

  async function setTimer() {
    if (epgEntry.isScheduled) {
      let res = await fetch(url + "record/" + epgEntry.eventId, {
        method: "DELETE",
      });

      epgEntry.isScheduled = false;
    } else {
      let res = await fetch(url + "record/" + epgEntry.eventId);

      epgEntry.isScheduled = true;
    }
  }

  function keydown(e) {
    if (e.code == "Escape") {
      epgEntry = null;
    }
  }
</script>

<svelte:window on:keydown={keydown} />

{#if epgEntry}
  <div transition:fade class="fixed z-50 inset-0 bg-gray-900 bg-opacity-50">
    <div
      transition:fly={{ y: 50 }}
      class="absolute inset-5 rounded border bg-gray-900 p-5 border-gray-900 "
    >
      <div class="flex items-center justify-start">
        <div class="bg-gray-800 rounded p-2 w-24">
          <img
            class="pointer-events-none"
            src="{url}{channel.iconUrl}"
            alt=""
          />
        </div>
        <div class="text-gray-100 font-bold text-2xl ml-4 flex items-center">
          {epgEntry.title}
          {#if epgEntry.isScheduled}
            <Icon
              class="text-red-600 ml-2 w-6 h-6"
              prefix="mdi"
              icon="record-circle"
            />
          {/if}
        </div>
      </div>
      <div class="text-gray-100 mt-6 text-lg font-medium">
        {epgEntry.description}
      </div>

      <div class="mt-5 flex gap-4 items-center">
        <button
          on:click={() => setTimer()}
          class="bg-red-800 hover:opacity-90 font-medium p-2 text-gray-100 rounded"
        >
          <div class="flex  justify-center items-center">
            {#if !epgEntry.isScheduled}
              <Icon
                class="w-8 h-8"
                prefix="heroicons-outline"
                icon="video-camera"
              />
              <div class="ml-2">Record</div>
            {:else}
              <Icon class="w-8 h-8" prefix="heroicons-outline" icon="trash" />
              <div class="ml-2">Delete</div>
            {/if}
          </div>
        </button>

        <button
          on:click={() => (epgEntry = null)}
          class="bg-gray-800 hover:opacity-90 p-2 font-medium text-gray-100 rounded"
        >
          <div class="flex justify-center items-center">
            <svg
              class="w-8 h-8"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              xmlns="http://www.w3.org/2000/svg"
              ><path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z"
              /></svg
            >
            <div class="ml-2">Close</div>
          </div>
        </button>
      </div>

      <!-- <div class="text-2xl">
        {epgEntry.title}
      </div> -->
    </div>
  </div>
{/if}
