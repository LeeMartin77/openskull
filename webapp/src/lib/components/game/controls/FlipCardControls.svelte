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

<h3>Reveal Cards</h3>
<div>
  {#if game.roundPlayerCardsRevealed[game.roundNumber - 1][game.activePlayerIndex].length !== game.roundCountPlayerCardsPlayed[game.roundNumber - 1][game.activePlayerIndex]}
    <button class="flip-button own-button" on:click={() => flipCard(game.activePlayerIndex)} disabled={game.activePlayerIndex !== game.playerIndex}
      >Reveal Own Card</button
    >
  {:else}
    <div class="other-buttons">
      {#each game.roundCountPlayerCardsPlayed[game.roundNumber - 1]
          .map((playedCount, i) => [playedCount, i])
          .filter((x) => x[1] !== game.activePlayerIndex) as [playedCount, i]}
          <button class="flip-button"
            on:click={() => flipCard(i)}
            disabled={game.activePlayerIndex !== game.playerIndex ||
              playedCount === game.roundPlayerCardsRevealed[game.roundNumber - 1][i].length}
            >Reveal {game.playerNicknames[i]} ({i})</button
          >
      {/each}
    </div>
  {/if}
</div>

<style>
  .flip-button {
    padding: 1em;
    font-weight: 700;
  }

  .own-button {
    width: 100%;
  }

  .other-buttons {
    text-align: center;
  }
</style>