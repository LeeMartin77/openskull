<script lang="ts">
  import { SKIP_VALUE } from 'src/config';

  import { type PublicGame, type PlayerGame, RoundPhase, CardType } from 'src/types/Game';
  import CardComponent from './CardComponent.svelte';

  export let game: PublicGame | PlayerGame;
  export let index: number;

  const revealedCards = (fnGame: PublicGame, fnIndex: number) =>
    fnGame.roundPlayerCardsRevealed[fnGame.roundNumber - 1][fnIndex];
  const revealedCardCount = (fnGame: PublicGame, fnIndex: number) =>
    fnGame.roundPlayerCardsRevealed[fnGame.roundNumber - 1][fnIndex].length;
  const unrevealedCardCount = (fnGame: PublicGame, fnIndex: number) =>
    fnGame.roundCountPlayerCardsPlayed[fnGame.roundNumber - 1][fnIndex] -
    fnGame.roundPlayerCardsRevealed[fnGame.roundNumber - 1][fnIndex].length;
  const unplayedCardCount = (fnGame: PublicGame, fnIndex: number) =>
    fnGame.currentCountPlayerCardsAvailable[fnIndex] -
    fnGame.roundCountPlayerCardsPlayed[fnGame.roundNumber - 1][fnIndex];
  const lostCardCount = (fnGame: PublicGame, fnIndex: number) =>
    fnGame.playerCardStartingCount - fnGame.currentCountPlayerCardsAvailable[fnIndex];
</script>

<div class="player-display {game.activePlayerIndex === index ? 'active-player' : ''}">
  <h3>
    {'playerIndex' in game && index === game.playerIndex ? '(You) ' : ''}{game.playerNicknames[index]}
  </h3>
  <h4>{game.roundWinners.filter((x) => x === index).length + ' point(s)'}</h4>
  <div class="card-display">
    {#each revealedCards(game, index) as card}
      <CardComponent display={CardType[card]} />
    {/each}
    {#each Array.from({ length: unrevealedCardCount(game, index) }, (v, i) => i) as _}
      <CardComponent display={'Back'} />
    {/each}
    {#each Array.from({ length: unplayedCardCount(game, index) }, (v, i) => i) as _}
      <CardComponent display={'Back'} disabled={true} />
    {/each}
    {#each Array.from({ length: lostCardCount(game, index) }, (v, i) => i) as _}
      <CardComponent display={'Back'} lost={true} />
    {/each}
  </div>

  {#if game.currentRoundPhase === RoundPhase.Bidding || (game.currentRoundPhase === RoundPhase.Flipping && game.activePlayerIndex === index)}
    <div>
      Bid: {game.currentBids[index] === SKIP_VALUE
        ? 'Withdrawn'
        : game.currentBids[index] === 0
        ? 'No Bid'
        : game.currentBids[index]}
    </div>
  {/if}
</div>

<style>
  .player-display {
    padding: 1em;
    margin: 1em;
    border-radius: 1em;
  }
  .active-player {
    border: 1px solid black;
  }
  .card-display {
    display: flex;
    flex-direction: row;
  }
</style>
