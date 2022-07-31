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

<Dialog open={newGameId !== undefined} complete={{ text: "Go to Game", action: () => {
  navigate('/games/' + newGameId);
  newGameId = undefined;
}}}>
  <p>Your new game is ready!</p>
</Dialog>
