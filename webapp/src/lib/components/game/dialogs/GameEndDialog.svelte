<script lang="ts">
  import type { PublicGame } from 'src/types/Game';
  import Dialog from 'src/lib/components/dialog/Dialog.svelte';

  export let open: boolean = false;
  export let game: PublicGame;
</script>

<Dialog {open}>
  <p>
    {game.playerNicknames[game.activePlayerIndex]} won after {game.roundCountPlayerCardsPlayed.length} rounds
  </p>
</Dialog>
