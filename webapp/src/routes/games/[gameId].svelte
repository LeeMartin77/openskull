<script lang="ts">
  import { HubConnectionBuilder } from '@microsoft/signalr';
  import { API_ROOT_URL } from 'src/config';
  import GameControls from 'src/lib/components/game/GameControls.svelte';
  import PublicGameDisplay from 'src/lib/components/game/PublicGameDisplay.svelte';
  import { generateUserHeaders } from 'src/stores/player';
  import type { PlayerGame, PublicGame } from 'src/types/Game';
  import type { OpenskullMessage } from 'src/types/OpenskullMessage';

  export let gameId: string;

  let game: PlayerGame | PublicGame = undefined;
  let loading: boolean = true;

  const updateGame = () => {
    loading = false;
    fetch(API_ROOT_URL + `/games/` + gameId, {
      headers: {
        ...generateUserHeaders()
      }
    })
      .then((res) => res.json())
      .then((parsed) => (game = parsed))
      .finally(() => (loading = false));
  };

  const messageHandler = (msg: OpenskullMessage) => {
    if (msg.activity === 'Turn' && msg.id === gameId) {
      updateGame();
    }
  };

  const gameConnection = new HubConnectionBuilder()
    .withUrl(API_ROOT_URL + '/game/ws')
    .build();

  gameConnection.on('send', messageHandler);

  gameConnection
    .start()
    .then(() => gameConnection.send('subscribeToGameId', gameId));

  updateGame();

  const idArray: number[] = [];
  if (game) {
    for (let i = 0; i < game.playerCount; i++) {
      idArray.push(i);
    }
  }
</script>

<div>
  Game view {gameId}
</div>
{#if game}
  <div>
    {#each Array.from({ length: game.playerCount }, (v, i) => i) as index}
      <PublicGameDisplay {game} {index} />
    {/each}
    {#if 'playerIndex' in game}
      <GameControls {game} />
    {/if}
  </div>
{:else if !loading}
  <div>Could not load game.</div>
{/if}
{#if loading}
  <div>Loading...</div>
{/if}
