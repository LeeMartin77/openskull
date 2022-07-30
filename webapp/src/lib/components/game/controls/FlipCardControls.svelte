<script lang="ts">
  import { API_ROOT_URL } from 'src/config';
  import { generateUserHeaders } from 'src/stores/player';
  import { TurnAction, type PlayerGame } from 'src/types/Game';

  export let game: PlayerGame;

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
