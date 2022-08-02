<script lang="ts">
  import type { PublicGame } from 'src/types/Game';
  import Dialog from 'src/lib/components/dialog/Dialog.svelte';
  import CardComponent from '../CardComponent.svelte';

  export let open: boolean = false;
  export let game: PublicGame;
</script>

<Dialog {open}>
  <h2 style="text-align: center;">{game.playerNicknames[game.activePlayerIndex]} Wins!</h2>
  <div class="card-container">
    <CardComponent display={'Flower'} />
  </div>
  <p style="text-align: center;">
    After {game.roundCountPlayerCardsPlayed.length} rounds
  </p>
</Dialog>

<style>
  .card-container {
    display: flex;
    align-items: center;
    justify-content: center;
  }
</style>
