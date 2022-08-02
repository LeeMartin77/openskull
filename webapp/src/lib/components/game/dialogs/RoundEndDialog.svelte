<script lang="ts">
  import type { PublicGame } from 'src/types/Game';
  import Dialog from 'src/lib/components/dialog/Dialog.svelte';
  import CardComponent from '../CardComponent.svelte';

  export let open: boolean = false;
  export let games: { prev: PublicGame; curr: PublicGame };
  const didWin = (gms: { prev: PublicGame; curr: PublicGame }): boolean => {
    return (
      gms.curr.roundWinners.filter((x) => x === gms.prev.activePlayerIndex).length >
      gms.prev.roundWinners.filter((x) => x === gms.prev.activePlayerIndex).length
    );
  };
</script>

<Dialog bind:open>
  <div class="card-container">
    <CardComponent display={didWin(games) ? 'Flower' : 'Skull'} />
  </div>
  <p style="text-align: center">
    Round {games.prev.roundNumber} finished when {games.prev.playerNicknames[games.prev.activePlayerIndex]}
    {didWin(games) ? 'revealed a final flower, winning' : 'revealed a skull, losing'} the round.
  </p>
</Dialog>

<style>
  .card-container {
    display: flex;
    align-items: center;
    justify-content: center;
  }
</style>
