<script lang="ts">
  import { API_ROOT_URL } from 'src/config';
  import { generateUserHeaders } from 'src/stores/player';
  import { CardState, TurnAction, type PlayerGame } from 'src/types/Game';

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
  </div>
</div>
