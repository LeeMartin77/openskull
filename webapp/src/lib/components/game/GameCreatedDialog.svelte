<script lang="ts">
  import { onDestroy } from 'svelte';
  import { navigate } from 'svelte-routing';

  import { playerConnection } from 'src/stores/player';
  import type { OpenskullMessage } from 'src/types/OpenskullMessage';

  let newGameId: string | undefined = undefined;

  const messageHandler = (msg: OpenskullMessage) => {
    if (msg.activity === 'GameCreated') {
      newGameId = msg.id;
    }
  };

  playerConnection.subscribe((con) => {
    con && con.on('send', messageHandler);
    return () => con && con.off('send', messageHandler);
  });
  onDestroy(() => $playerConnection.off('send', messageHandler));
</script>

{#if newGameId !== undefined}
  <div>
    <button
      on:click={() => {
        navigate('/games/' + newGameId);
        newGameId = undefined;
      }}>Go to New Game</button
    >
  </div>
{:else}
  <div>No new game link</div>
{/if}
