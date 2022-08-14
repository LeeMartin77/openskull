<script lang="ts">
  import { CURRENT_SOLO_GAME, SOLOGAME_BOTS, type SoloGameBot } from 'src/stores/solo';
  import { v4 as randomUUID } from 'uuid';
  import { generateUserHeaders, OPENSKULL_USER_ID } from 'src/stores/player';
  import { HubConnectionBuilder } from '@microsoft/signalr';
  import { API_ROOT_URL } from 'src/config';
  import GameInterface from './[gameId].svelte';
  import type { OpenskullMessage } from 'src/types/OpenskullMessage';
  import type { PlayerGame } from 'src/types/Game';
  import { playRoboTurn } from 'src/lib/functions/RoboTurn';

  let soloGameBots: SoloGameBot[] = [];
  let soloGameId: string;
  let gameComplete: boolean = false;

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

  CURRENT_SOLO_GAME.subscribe((soloGameIdVal) => {
    soloGameId = soloGameIdVal;
  });

  const createSoloGame = () => {
    CURRENT_SOLO_GAME.set(null);
    gameComplete = false;
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

  const handleGameUpdate = () => {
    soloGameBots.forEach((bot) => {
      fetch(API_ROOT_URL + `/games/` + soloGameId, {
        headers: {
          ...generateUserHeaders(bot.id, bot.secret)
        }
      })
        .then((res) => res.json())
        .then((parsed: PlayerGame) => {
          gameComplete = parsed.gameComplete;
          if (parsed.gameComplete) {
            // game has ended
            return;
          }
          if (parsed.activePlayerIndex !== parsed.playerIndex) {
            // It's not this bot's turn
            return;
          }
          playRoboTurn(bot, parsed);
        });
    });
  };

  if (soloGameId) {
    const turnHandler = (msg: OpenskullMessage) => {
      if (msg.activity === 'Turn' && msg.id === soloGameId) {
        handleGameUpdate();
      }
    };

    gameConnection.on('send', turnHandler);
    handleGameUpdate();
  }
</script>

{#if soloGameBots.length === 0}
  <h2>Building Bots...</h2>
{:else if !soloGameId}
  <button on:click={createSoloGame}>Create New Solo Game</button>
{:else}
  {#if gameComplete}
    <button on:click={createSoloGame}>Create New Solo Game</button>
  {/if}
  <GameInterface gameId={soloGameId} {gameConnection} />
{/if}

<style>
</style>
