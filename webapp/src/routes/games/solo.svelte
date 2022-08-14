<script lang="ts">
  import { CURRENT_SOLO_GAME, SOLOGAME_BOTS, type SoloGameBot } from 'src/stores/solo';
  import { v4 as randomUUID } from 'uuid';
  import { generateUserHeaders, OPENSKULL_USER_ID } from 'src/stores/player';
  import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
  import { API_ROOT_URL } from 'src/config';
  import GameInterface from './[gameId].svelte';
  import type { OpenskullMessage } from 'src/types/OpenskullMessage';
  import type { PlayerGame } from 'src/types/Game';
  import { playRoboTurn } from 'src/lib/functions/RoboTurn';

  let soloGameBots: SoloGameBot[] = [];
  let soloGameId: string;
  let gameComplete: boolean = false;
  let gameExists: boolean = false;
  let loading: boolean = true;

  const gameConnection = new HubConnectionBuilder().withUrl(API_ROOT_URL + '/game/ws').build();

  SOLOGAME_BOTS.subscribe((soloGameBotsVal) => {
    if (soloGameBotsVal.length === 0) {
      const newBots: SoloGameBot[] = [
        { id: randomUUID(), secret: randomUUID(), nickname: 'Botty' },
        { id: randomUUID(), secret: randomUUID(), nickname: 'Droidi' }
      ];
      SOLOGAME_BOTS.set(newBots);
    } else {
      const altPlayerConnection = new HubConnectionBuilder().withUrl(API_ROOT_URL + '/player/ws').build();

      altPlayerConnection
        .start()
        .then(() =>
          Promise.all(
            soloGameBotsVal.map((bot) => altPlayerConnection.send('UpdateNickname', bot.id, bot.secret, bot.nickname))
          ).finally(() => altPlayerConnection.stop())
        );

      soloGameBots = soloGameBotsVal;
    }
  });

  let turnHandler;

  CURRENT_SOLO_GAME.subscribe((soloGameIdVal) => {
    if (soloGameIdVal) {
      fetch(API_ROOT_URL + '/games/' + soloGameIdVal, { headers: generateUserHeaders() }).then((res) => {
        if (res.status !== 200) {
          CURRENT_SOLO_GAME.set(null);
          gameExists = false;
        } else {
          gameExists = true;
          if (turnHandler) {
            gameConnection.off('send', turnHandler);
          }
          turnHandler = (msg: OpenskullMessage) => {
            if (msg.activity === 'Turn' && msg.id === soloGameIdVal) {
              handleGameUpdate(soloGameIdVal);
            }
          };

          gameConnection.on('send', turnHandler);
          handleGameUpdate(soloGameIdVal);
        }
        loading = false;
      });
    }
    soloGameId = soloGameIdVal;
  });

  const createSoloGame = () => {
    CURRENT_SOLO_GAME.set(null);
    gameComplete = false;
    if (gameConnection.state !== HubConnectionState.Disconnected) {
      gameConnection.stop();
    }
    fetch(`${API_ROOT_URL}/games`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...generateUserHeaders() },
      body: JSON.stringify({
        playerIds: [OPENSKULL_USER_ID, ...soloGameBots.map((x) => x.id)]
      })
    })
      .then((res) => res.json())
      .then((gameId) => CURRENT_SOLO_GAME.set(gameId));
  };

  const handleGameUpdate = (gameId: string) => {
    const botviews = soloGameBots.map((bot) =>
      fetch(API_ROOT_URL + `/games/` + gameId, {
        headers: {
          ...generateUserHeaders(bot.id, bot.secret)
        }
      }).then((res) => res.json() as unknown as PlayerGame)
    );
    Promise.all(botviews).then((botgameviews) => {
      botgameviews.forEach((parsed, i) => {
        gameComplete = parsed.gameComplete;
        if (parsed.gameComplete) {
          // game has ended
          return;
        }
        if (parsed.activePlayerIndex === parsed.playerIndex) {
          // It's this bot's turn
          playRoboTurn(soloGameBots[i], parsed);
        }
      });
    });
  };
</script>

{#if soloGameBots.length === 0}
  <h2>Building Bots...</h2>
{:else if !soloGameId || (!loading && !gameExists)}
  <button on:click={createSoloGame}>Create New Solo Game</button>
{:else if gameExists}
  {#if gameComplete}
    <button on:click={createSoloGame}>Create New Solo Game</button>
  {/if}
  <GameInterface gameId={soloGameId} {gameConnection} />
{:else}
  <h1>Loading...</h1>
{/if}

<style>
</style>
