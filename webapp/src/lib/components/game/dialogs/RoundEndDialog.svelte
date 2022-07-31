<script lang="ts">
  import type { PublicGame } from 'src/types/Game';
  import Dialog from 'src/lib/components/dialog/Dialog.svelte';

  export let open: boolean = false;
  export let games: { prev: PublicGame; curr: PublicGame };
  const didWin = (gms: { prev: PublicGame; curr: PublicGame }): boolean => {
    return (
      gms.curr.roundWinners.filter((x) => x === gms.prev.activePlayerIndex)
        .length >
      gms.prev.roundWinners.filter((x) => x === gms.prev.activePlayerIndex)
        .length
    );
  };
</script>

<Dialog {open}>
  <p>
    Round {games.prev.roundNumber} finished when {games.prev.playerNicknames[
      games.prev.activePlayerIndex
    ]} stopped revealing cards, {didWin(games) ? 'winning' : 'losing'} the round.
  </p>
</Dialog>
