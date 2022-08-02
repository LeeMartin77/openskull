<script lang="ts">
  import { onDestroy } from 'svelte';
  import { navigate } from 'svelte-routing';

  import { playerConnection } from 'src/stores/player';
  import type { OpenskullMessage } from 'src/types/OpenskullMessage';
  import Dialog from '../dialog/Dialog.svelte';

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

<Dialog hideClose={true} open={newGameId !== undefined}>
  <p style="font-weight: 700">Your new game is ready!</p>
  <button
    class="new-game-button"
    on:click={() => {
      navigate('/games/' + newGameId);
      newGameId = undefined;
    }}>Go to Game!</button
  >
</Dialog>

<style>
  .new-game-button {
    width: 100%;
    font-weight: 700;
    padding: 1em;
    border: 1px solid black;
  }
</style>
