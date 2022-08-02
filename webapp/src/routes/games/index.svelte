<script lang="ts">
  import { API_ROOT_URL } from 'src/config';
  import { generateUserHeaders } from 'src/stores/player';
  import type { PublicGame } from 'src/types/Game';
  import { navigate } from 'svelte-routing';
  let loading = true;
  let games: PublicGame[] = [];
  let pageIndex: number = 0;
  let pageLength: number = 25;
  fetch(
    API_ROOT_URL +
      `/games?` +
      new URLSearchParams({
        pageIndex: pageIndex.toString(),
        pageLength: pageLength.toString()
      }),
    {
      headers: {
        ...generateUserHeaders()
      }
    }
  )
    .then((res) => res.json())
    .then((parsed) => (games = parsed))
    .finally(() => (loading = false));
</script>

{#if !loading}
  <div>
    {#if games.length === 0}
      No Games
    {:else}
      {#each games as game (game.id)}
        <button class="game-button" on:click={() => navigate('/games/' + game.id)}
          >{game.playerCount} Player Game: {game.gameComplete ? 'Completed' : 'Ongoing'}</button
        >
      {/each}
    {/if}
  </div>
{:else}
  <div>Loading...</div>
{/if}

<style>
  .game-button {
    border: none;
    width: 100%;
    font-weight: 700;
    padding: 1em;
    margin: 0.5em 0;
  }

  .game-button:hover {
    background-color: lightgray;
  }
</style>
