<script lang="ts">
  import { HubConnectionBuilder } from '@microsoft/signalr';
  import { API_ROOT_URL, generateUserHeaders } from 'src/config';
  import { OPENSKULL_USER_ID, OPENSKULL_USER_SECRET } from 'src/stores/player';
  import type { PlayerGame, PublicGame } from 'src/types/Game';
  import type { OpenskullMessage } from 'src/types/OpenskullMessage';

  export let gameId: string;

  let game: PlayerGame | PublicGame = undefined;
  let loading: boolean = true;

  const updateGame = () => {
    loading = false;
    fetch(API_ROOT_URL + `/games/` + gameId, {
      headers: {
        ...generateUserHeaders(OPENSKULL_USER_ID, OPENSKULL_USER_SECRET)
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
</script>

<div>
  Game view {gameId}
</div>
{#if game}
  <div>
    {JSON.stringify(game)}
  </div>
{/if}
{#if loading}
  <div>Loading...</div>
{/if}
