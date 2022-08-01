<script lang="ts">
  import { onDestroy } from 'svelte';

  import { playerConnection, playerConnectionState } from 'src/stores/player';
  import type { OpenskullMessage } from 'src/types/OpenskullMessage';

  let messages: OpenskullMessage[] = [];

  const messageHandler = (msg: OpenskullMessage) => {
    messages = [...messages, msg];
  };

  playerConnection.subscribe((con) => {
    con && con.on('send', messageHandler);
    return () => con && con.off('send', messageHandler);
  });
  onDestroy(() => $playerConnection.off('send', messageHandler));
</script>
<div class="connection-status">
  {#if $playerConnectionState === "Connected"}
    <div class="connection-status-icon connected"></div>
  {:else if $playerConnectionState === "Disconnected"}
    <div class="connection-status-icon disconnected"></div>
  {:else}
    <div class="connection-status-icon changing"></div>
  {/if}
</div>
<!-- {#each messages as message, i}
  <div>
    Received {JSON.stringify(message)}
    <button on:click={() => { messages.splice(i, 1);  messages = messages }}>Dismiss</button>
  </div>
{/each} -->
<style>
  .connection-status {
    position: fixed;
    top: -1px;
    right: -1px;
    padding: 0.25em;
    border-radius: 0 0 0 0.5em;
    border: 1px gray solid;
  }
  .connection-status-icon {
    background-color: black;
    border-radius: 1em;
    width: 0.75em;
    height: 0.75em;
  }
  .connected {
    background-color: green;
  }
  .disconnected {
    background-color: red;
  }
  .changing {
    background-color: yellow;
  }
</style>