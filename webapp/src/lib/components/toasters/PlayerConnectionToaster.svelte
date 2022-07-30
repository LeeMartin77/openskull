<script lang="ts">
  import { onDestroy } from "svelte";

  import { playerConnection, playerConnectionState } from "src/stores/player"
  import type { OpenskullMessage } from "src/types/OpenskullMessage";

  let messages: OpenskullMessage[] = [];

  const messageHandler = (msg: OpenskullMessage) => {
    messages = [...messages, msg]
  }

  playerConnection.subscribe(con => {
    con.on("send", messageHandler)
    return () => con.off("send", messageHandler);
  })
  onDestroy(() => $playerConnection.off("send", messageHandler))
</script>

Player Connection State: {$playerConnectionState}

{#each messages as message, i}
  <div>
    Received {JSON.stringify(message)}
    <button on:click={() => { messages.splice(i, 1);  messages = messages }}>Dismiss</button>
  </div>
{/each}