<script lang="ts">
  import { RoundPhase, type PlayerGame } from 'src/types/Game';
  import Dialog from 'src/lib/components/dialog/Dialog.svelte';
  import FlipCardControls from 'src/lib/components/game/controls/FlipCardControls.svelte';
  import PlaceBidControls from 'src/lib/components/game/controls/PlaceBidControls.svelte';
  import PlayCardControls from 'src/lib/components/game/controls/PlayCardControls.svelte';
  import CardComponent from './CardComponent.svelte';

  export let game: PlayerGame;

  const instructionVersion = 'v1.0';
  const instructionVersionKey = 'Openskull_instructionversionseen';
  let instructionsDialogVersionSeen = localStorage.getItem(instructionVersionKey);
  localStorage.setItem(instructionVersionKey, instructionVersion);

  let helpOpen = instructionVersion !== instructionsDialogVersionSeen;
  let helpCardScale = 0.7;
</script>

<div>
  <button class="help-button" on:click={() => (helpOpen = true)}>?</button>
  <Dialog bind:open={helpOpen}>
    <h3>How to Play</h3>
    <ul class="instructions">
      <li>Skull is played over muliple rounds made up of playing cards, bidding, then revealing played cards.</li>
      <li>To win, you must earn two points.</li>
      <li>To earn a point you need to successfully reveal the number of cards you bid (starting with your own).</li>
      <li>If you reveal a skull card, you lose one of your 4 cards and don't score a point.</li>
    </ul>
    <h3>Card Display</h3>
    <p>
      The below diplay shows, in order: A revealed flower, an unrevealed played card, an unplayed card, and a lost Card
    </p>
    <div class="help-card-display">
      <CardComponent display="Flower" scale={helpCardScale} />
      <CardComponent display="Back" scale={helpCardScale} />
      <CardComponent display="Back" disabled={true} scale={helpCardScale} />
      <CardComponent display="Back" lost={true} scale={helpCardScale} />
    </div>
  </Dialog>
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

<style>
  .help-button {
    padding: 0.5em;
    border-radius: 2em;
    border: 3px solid black;
    font-weight: 700;
    width: 2.5em;
    height: 2.5em;
    position: absolute;
    right: 1em;
    top: 1em;
  }

  ul.instructions {
    list-style-type: none;
    padding-left: 0;
  }

  .instructions li {
    margin-bottom: 0.5em;
  }

  .help-card-display {
    display: flex;
  }
</style>
