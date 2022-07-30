<script lang="ts">
  import { SKIP_VALUE } from 'src/config';

  import { type PublicGame, type PlayerGame, RoundPhase } from 'src/types/Game';

  export let game: PublicGame | PlayerGame;
  export let index: number;
</script>

<div class={game.activePlayerIndex === index ? 'active-player' : ''}>
  <h3>
    {'playerIndex' in game && index === game.playerIndex ? '(You) ' : ''}{game
      .playerNicknames[index]}
  </h3>
  <h4>{game.roundWinners.filter((x) => x === index).length + ' point(s)'}</h4>
  <ul>
    <li>
      Revealed: {JSON.stringify(
        game.roundPlayerCardsRevealed[game.roundNumber - 1][index].length
      )} ({game.roundPlayerCardsRevealed[game.roundNumber - 1][index].length})
    </li>
    <li>
      Unrevealed: {game.roundCountPlayerCardsPlayed[game.roundNumber - 1][
        index
      ] - game.roundPlayerCardsRevealed[game.roundNumber - 1][index].length}
    </li>
    <li>
      Unplayed: {game.currentCountPlayerCardsAvailable[index] -
        game.roundCountPlayerCardsPlayed[game.roundNumber - 1][index]}
    </li>
    <li>
      Lost: {game.playerCardStartingCount -
        game.currentCountPlayerCardsAvailable[index]}
    </li>
  </ul>
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
  .active-player {
    border: 1px solid black;
  }
</style>
