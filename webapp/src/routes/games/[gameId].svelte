<script lang="ts">
  import { HubConnectionBuilder } from '@microsoft/signalr';
  import { API_ROOT_URL } from 'src/config';
  import GameEndDialog from 'src/lib/components/game/dialogs/GameEndDialog.svelte';
  import RoundEndDialog from 'src/lib/components/game/dialogs/RoundEndDialog.svelte';
  import GameControls from 'src/lib/components/game/GameControls.svelte';
  import PublicGameDisplay from 'src/lib/components/game/PublicGameDisplay.svelte';
  import { generateUserHeaders } from 'src/stores/player';
  import type { PlayerGame, PublicGame } from 'src/types/Game';
  import type { OpenskullMessage } from 'src/types/OpenskullMessage';

  export let gameId: string;

  let game: PlayerGame | PublicGame = undefined;
  let loading: boolean = true;

  let gameComplete: boolean = false;

  let roundEnd: { prev: PublicGame; curr: PublicGame } | undefined = undefined;

  const updateGame = () => {
    loading = false;
    fetch(API_ROOT_URL + `/games/` + gameId, {
      headers: {
        ...generateUserHeaders()
      }
    })
      .then((res) => res.json())
      .then((parsed: PlayerGame | PublicGame) => {
        // game has ended
        if (parsed.gameComplete) {
          gameComplete = false;
          gameComplete = true;
        }
        if (!parsed.gameComplete && game && game.roundNumber !== parsed.roundNumber) {
          roundEnd = undefined;
          roundEnd = {
            prev: game,
            curr: parsed
          };
        }
        // round has ended
        game = parsed;
      })
      .finally(() => (loading = false));
  };

  const messageHandler = (msg: OpenskullMessage) => {
    if (msg.activity === 'Turn' && msg.id === gameId) {
      updateGame();
    }
  };

  const gameConnection = new HubConnectionBuilder().withUrl(API_ROOT_URL + '/game/ws').build();

  gameConnection.on('send', messageHandler);

  gameConnection.start().then(() => gameConnection.send('subscribeToGameId', gameId));

  updateGame();
</script>

{#if game}
  <GameEndDialog open={gameComplete} {game} />
  <RoundEndDialog open={roundEnd !== undefined} games={roundEnd} />
  <div>
    {#each Array.from({ length: game.playerCount }, (v, i) => i) as index}
      <PublicGameDisplay {game} {index} />
    {/each}
    {#if 'playerIndex' in game && !game.gameComplete}
      <GameControls {game} />
    {/if}
  </div>
{:else if !loading}
  <div>Could not load game.</div>
{/if}
{#if loading}
  <div>Loading...</div>
{/if}
