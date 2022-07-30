<script lang="ts">
  import { API_ROOT_URL, SKIP_VALUE } from 'src/config';
  import { generateUserHeaders } from 'src/stores/player';
  import { CardState, TurnAction, type PlayerGame } from 'src/types/Game';

  export let game: PlayerGame;

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

  const flipCard = (targetPlayerIndex: number) => {
    fetch(`${API_ROOT_URL}/games/${game.id}/turn`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...generateUserHeaders() },
      body: JSON.stringify({
        action: TurnAction[TurnAction.Flip],
        targetPlayerIndex
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
            game.playerRoundCardIdsPlayed[game.roundNumber - 1].includes(
              card.id
            )}>{card.type}</button
        >
      {/each}
    </div>
    <h3>Place Bid</h3>
    <div>
      <button
        on:click={() => placeBid(SKIP_VALUE)}
        disabled={game.activePlayerIndex !== game.playerIndex}>Withdraw</button
      >
      {#each Array.from({ length: game.roundCountPlayerCardsPlayed[game.roundNumber - 1].reduce((prev, curr) => prev + curr, 1) }, (v, i) => i) as bidNumber}
        <button
          on:click={() => placeBid(bidNumber)}
          disabled={game.activePlayerIndex !== game.playerIndex}
          >{bidNumber}</button
        >
      {/each}
    </div>
    <h3>Flip Cards</h3>
    <div>
      {#each Array.from({ length: game.playerCount }, (v, i) => i) as targetPlayerIndex}
        <button
          on:click={() => flipCard(targetPlayerIndex)}
          disabled={game.activePlayerIndex !== game.playerIndex}
          >{game.playerNicknames[targetPlayerIndex]} ({targetPlayerIndex})</button
        >
      {/each}
    </div>
  </div>
</div>
