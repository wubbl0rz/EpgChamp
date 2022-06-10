<script>
  import "../app.css";
  import "@fontsource/roboto-mono/500.css";
  import "@fontsource/inter/100.css";
  import "@fontsource/inter/200.css";
  import "@fontsource/inter/300.css";
  import "@fontsource/inter/400.css";
  import "@fontsource/inter/500.css";
  import "@fontsource/inter/600.css";
  import "@fontsource/inter/700.css";
  import "@fontsource/inter/800.css";
  import "@fontsource/inter/900.css";
</script>

<slot />

<!-- preload fonts -->
<div class="absolute top-0 left-0 opacity-0">
  <div class="font-bold">1</div>
  <div class="font-medium">2</div>
  <div class="font-normal">3</div>
</div>

<style global>
  body {
    font-family: "Inter", sans-serif;
  }
</style>
