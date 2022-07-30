<script lang="ts">
  import { API_ROOT_URL, SKIP_VALUE } from 'src/config';
  import { generateUserHeaders } from 'src/stores/player';
  import { TurnAction, type PlayerGame } from 'src/types/Game';

  export let game: PlayerGame;

  const placeBid = (bid: number) => {
    fetch(`${API_ROOT_URL}/games/${game.id}/turn`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...generateUserHeaders() },
      body: JSON.stringify({
        action: TurnAction[TurnAction.Bid],
        bid
      })
    });
  };
</script>

<h3>Place Bid</h3>
<div>
  <button
    on:click={() => placeBid(SKIP_VALUE)}
    disabled={game.activePlayerIndex !== game.playerIndex}>Withdraw</button
  >
  {#each Array.from({ length: game.roundCountPlayerCardsPlayed[game.roundNumber - 1].reduce((prev, curr) => prev + curr, 1) }, (v, i) => i) as bidNumber}
    <button
      on:click={() => placeBid(bidNumber)}
      disabled={game.activePlayerIndex !== game.playerIndex}>{bidNumber}</button
    >
  {/each}
</div>
