<script context="module">
  import * as heroiconsOutline from "@iconify/json/json/heroicons-outline.json";
  import * as mdiIcons from "@iconify/json/json/mdi.json";

  let iconRegistry = {};

  function addIconsToRegistry(iconSet) {
    let prefix = iconSet.prefix;
    iconRegistry[prefix] = iconSet.icons;
  }

  addIconsToRegistry(mdiIcons);
  addIconsToRegistry(heroiconsOutline);
</script>

<script>
  let className = "";

  export { className as class };
  export let prefix;
  export let icon;
</script>

<div class={className}>
  <svg viewBox="0 0 24 24">
    {@html iconRegistry[prefix][icon].body}
  </svg>
</div>
