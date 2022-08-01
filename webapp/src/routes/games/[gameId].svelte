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
  <div class="game-container {'playerIndex' in game && !game.gameComplete ? "game-container-contoller-offset" : ""}">
    {#each Array.from({ length: game.playerCount }, (v, i) => i) as index}
      <div>
        <PublicGameDisplay {game} {index} />
      </div>
    {/each}
  </div>
  {#if 'playerIndex' in game && !game.gameComplete}
  <div class="control-container">
      <div>
        <GameControls {game} />
      </div>
  </div>
  {/if}
{:else if !loading}
  <div>Could not load game.</div>
{/if}
{#if loading}
  <div>Loading...</div>
{/if}

<style>
  .game-container {
    display: grid;
    grid-template-columns: 1fr 1fr 1fr;
    gap: 0.5em;
    justify-items: center;
    align-items: center;
  }

  .game-container-contoller-offset {
    padding-bottom: 420px
  }

  .control-container {
    position: fixed;
    left: 2em;
    right: 2em;
    bottom: -1px;
    background-color: white;
    display:flex;
    justify-items: center;
    align-items: center;
    padding: 1em 0 4em 0;
    border: 1px black solid;
    border-radius: 1em 1em 0 0;
  }

  .control-container > div {
    margin: auto;
  }

  @media only screen and (max-width: 960px) {
    .game-container {
      grid-template-columns: 1fr 1fr;
    }
  }

  @media only screen and (max-width: 480px) {
    .game-container {
      grid-template-columns: 1fr;
    }
  }
</style>