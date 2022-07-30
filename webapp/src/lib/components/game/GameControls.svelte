<script lang="ts">
  import { API_ROOT_URL, SKIP_VALUE } from 'src/config';
  import { generateUserHeaders } from 'src/stores/player';
  import { CardState, TurnAction, type PlayerGame } from 'src/types/Game';
  import { prevent_default } from 'svelte/internal';

  export let game: PlayerGame;
  const roundIndex = game.roundNumber - 1;
  const playCard = (cardId: string) => {
    fetch(`${API_ROOT_URL}/games/${game.id}/turn`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...generateUserHeaders() },
      body: JSON.stringify({
        action: TurnAction[TurnAction.Card],
        cardId
      })
    });
  };

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

<div>
  <div>
    <h3>Play Card</h3>
    <div>
      {#each game.playerCards as card (card.id)}
        <button
          on:click={() => playCard(card.id)}
          disabled={card.state === CardState.Discarded ||
            game.activePlayerIndex !== game.playerIndex ||
            game.playerRoundCardIdsPlayed[roundIndex].includes(card.id)}
          >{card.type}</button
        >
      {/each}
    </div>
    <h3>Place Bid</h3>
    <div>
      <button
        on:click={() => placeBid(SKIP_VALUE)}
        disabled={game.activePlayerIndex !== game.playerIndex}>Withdraw</button
      >
      {#each Array.from({ length: game.roundCountPlayerCardsPlayed[roundIndex].reduce((prev, curr) => prev + curr, 1) }, (v, i) => i) as bidNumber}
        <button
          on:click={() => placeBid(bidNumber)}
          disabled={game.activePlayerIndex !== game.playerIndex}
          >{bidNumber}</button
        >
      {/each}
    </div>
  </div>
</div>
