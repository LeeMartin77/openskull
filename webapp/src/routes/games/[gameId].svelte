<script lang="ts">
  import { HubConnectionBuilder } from '@microsoft/signalr';
  import { API_ROOT_URL } from 'src/config';
  import Dialog from 'src/lib/components/dialog/Dialog.svelte';
  import GameEndDialog from 'src/lib/components/game/dialogs/GameEndDialog.svelte';
  import RoundEndDialog from 'src/lib/components/game/dialogs/RoundEndDialog.svelte';
  import GameControls from 'src/lib/components/game/GameControls.svelte';
  import PublicGameDisplay from 'src/lib/components/game/PublicGameDisplay.svelte';
  import { generateUserHeaders } from 'src/stores/player';
  import type { PlayerGame, PublicGame } from 'src/types/Game';
  import type { OpenskullMessage } from 'src/types/OpenskullMessage';

  export let gameId: string;
  export let gameConnection = new HubConnectionBuilder().withUrl(API_ROOT_URL + '/game/ws').build();

  let game: PlayerGame | PublicGame = undefined;
  let loading: boolean = true;

  let gameComplete: boolean = false;

  let roundEnd: { prev: PublicGame; curr: PublicGame } | undefined = undefined;

  const updateGame = () => {
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

  gameConnection.on('send', messageHandler);

  gameConnection.start().then(() => gameConnection.send('subscribeToGameId', gameId));

  let gameConnectionLost = false;
  gameConnection.onclose(() => gameConnectionLost = true);

  updateGame();
</script>

<Dialog open={gameConnectionLost} hideClose={true}>
  <h4>Connection to OpenSkull has been lost - please refresh the page.</h4>
</Dialog>

{#if game}
  <GameEndDialog open={gameComplete} {game} />
  <RoundEndDialog open={roundEnd !== undefined} games={roundEnd} />
  <div class="game-container {'playerIndex' in game && !game.gameComplete ? 'game-container-contoller-offset' : ''}">
    {#each Array.from({ length: game.playerCount }, (v, i) => i) as index}
      <div>
        <PublicGameDisplay {game} {index} />
      </div>
    {/each}
  </div>
  {#if 'playerIndex' in game && !game.gameComplete}
    <div class="control-container {game.activePlayerIndex === game.playerIndex ? 'active-player' : ''}">
      {#if game.activePlayerIndex !== game.playerIndex}
        <div class="control-blocker">
          <h2>Waiting for {game.playerNicknames[game.activePlayerIndex]} to Play</h2>
        </div>
      {:else}
        <div>
          <GameControls {game} />
        </div>
      {/if}
    </div>
  {/if}
{:else if !loading}
  <div>Could not load game.</div>
{:else}
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
    padding-bottom: 420px;
  }

  .control-container {
    position: fixed;
    left: 2em;
    right: 2em;
    bottom: -4px;
    background-color: white;
    display: flex;
    justify-items: center;
    align-items: center;
    padding: 1em 0 4em 0;
    border: 1px black solid;
    border-radius: 1em 1em 0 0;
  }

  @keyframes border-pulsate {
    0% {
      border-color: rgba(0, 0, 0, 1);
    }
    50% {
      border-color: rgba(200, 200, 200, 1);
    }
    100% {
      border-color: rgba(0, 0, 0, 1);
    }
  }

  .control-container.active-player {
    border: 4px rgba(0, 0, 0, 0) dashed;
    animation: border-pulsate 2s infinite;
  }

  .control-blocker {
    background-color: rgba(150, 150, 150, 0.5);
    width: 100%;
    text-align: center;
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
