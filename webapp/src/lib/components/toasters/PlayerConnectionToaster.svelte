<script lang="ts">
  import { onDestroy } from 'svelte';

  import { playerConnection, playerConnectionState } from 'src/stores/player';
  import type { OpenskullMessage } from 'src/types/OpenskullMessage';
  import Dialog from '../dialog/Dialog.svelte';

  let refreshDialog: boolean = false;

  playerConnection.subscribe((con) => {
    con && con.onclose(() => (refreshDialog = true));
    return () => {};
  });
  onDestroy(() => {});
</script>

<div class="connection-status">
  {#if $playerConnectionState === 'Connected'}
    <div class="connection-status-icon connected" />
  {:else if $playerConnectionState === 'Disconnected'}
    <div class="connection-status-icon disconnected" />
  {:else}
    <div class="connection-status-icon changing" />
  {/if}
</div>
<Dialog open={refreshDialog} hideClose={true}>
  <h4>Connection to OpenSkull has been lost - please refresh the page.</h4>
</Dialog>

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
