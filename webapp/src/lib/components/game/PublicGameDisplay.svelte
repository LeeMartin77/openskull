<script lang="ts">
  import { SKIP_VALUE } from 'src/config';

  import { type PublicGame, type PlayerGame, RoundPhase } from 'src/types/Game';

  export let game: PublicGame | PlayerGame;
  export let index: number;

  const isMe = 'playerIndex' in game && index === game.playerIndex;
  const roundIndex = game.roundNumber - 1;
  const wins = game.roundWinners.filter((x) => x === index).length;
  const playerRevealedCards = game.roundPlayerCardsRevealed[roundIndex][index];
  const playedRevealedCardsCount =
    game.roundPlayerCardsRevealed[roundIndex][index].length;
  const unrevealedCardsPlayed =
    game.roundCountPlayerCardsPlayed[roundIndex][index] -
    playedRevealedCardsCount;
  const cardsUnplayed =
    game.currentCountPlayerCardsAvailable[index] -
    game.roundCountPlayerCardsPlayed[roundIndex][index];
  const cardsLost =
    game.playerCardStartingCount - game.currentCountPlayerCardsAvailable[index];
  const currentBid =
    game.currentBids[index] === SKIP_VALUE
      ? 'Withdrawn'
      : game.currentBids[index] === 0
      ? 'No Bid'
      : game.currentBids[index];
</script>

<div class={game.activePlayerIndex === index ? 'active-player' : ''}>
  <h3>{isMe ? '(You) ' : ''}{game.playerNicknames[index]}</h3>
  <h4>{wins + ' point(s)'}</h4>
  <ul>
    <li>Revealed: {JSON.stringify(playerRevealedCards)}</li>
    <li>Unrevealed: {unrevealedCardsPlayed}</li>
    <li>Unplayed: {cardsUnplayed}</li>
    <li>Lost: {cardsLost}</li>
  </ul>
  {#if game.currentRoundPhase === RoundPhase.Bidding || (game.currentRoundPhase === RoundPhase.Flipping && game.activePlayerIndex === index)}
    <div>Bid: {currentBid}</div>
  {/if}
</div>

<style>
  .active-player {
    border: 1px solid black;
  }
</style>
