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
  {#if games.length === 0}
    <div>No Games</div>
  {:else}
    <ul>
      {#each games as game (game.id)}
        <li>
          <button on:click={() => navigate('/games/' + game.id)}
            >{game.playerCount} Game: {game.gameComplete
              ? 'Completed'
              : 'Ongoing'}</button
          >
        </li>
      {/each}
    </ul>
  {/if}
{:else}
  <div>Loading...</div>
{/if}
