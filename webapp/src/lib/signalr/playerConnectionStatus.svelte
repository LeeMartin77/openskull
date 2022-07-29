<script lang="ts">
import { onDestroy } from "svelte";

  import { playerConnection, playerConnectionState } from "./playerConnection"
  $playerConnection.on("send", console.log)
  onDestroy(() => $playerConnection.off("send", console.log))
</script>

Player Connection State: {$playerConnectionState}