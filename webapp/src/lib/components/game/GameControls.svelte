<script lang="ts">
  import { RoundPhase, type PlayerGame } from 'src/types/Game';
  import FlipCardControls from './controls/FlipCardControls.svelte';
  import PlaceBidControls from './controls/PlaceBidControls.svelte';
  import PlayCardControls from './controls/PlayCardControls.svelte';

  export let game: PlayerGame;
</script>

<div>
  {#if [RoundPhase.PlayFirstCards, RoundPhase.PlayCards].includes(game.currentRoundPhase)}
    <PlayCardControls {game} />
  {/if}

  {#if [RoundPhase.PlayCards, RoundPhase.Bidding].includes(game.currentRoundPhase)}
    <PlaceBidControls {game} />
  {/if}
  {#if RoundPhase.Flipping === game.currentRoundPhase && game.activePlayerIndex === game.playerIndex}
    <FlipCardControls {game} />
  {/if}
</div>
