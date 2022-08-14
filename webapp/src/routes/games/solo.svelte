<script lang="ts">
  import { CURRENT_SOLO_GAME, SOLOGAME_BOTS, type SoloGameBot } from 'src/stores/solo';
  let soloGameBots: SoloGameBot[] = [];
  let soloGameId: string;
  SOLOGAME_BOTS.subscribe((soloGameBotsVal) => {
    if (soloGameBotsVal.length === 0) {
      // create bots + set -> will retrigger this
    } else {
      soloGameBots = soloGameBotsVal;
    }
  });
  if (soloGameBots) {
    CURRENT_SOLO_GAME.subscribe((soloGameIdVal) => {
      soloGameId = soloGameIdVal;
    });
  }

  const createSoloGame = () => {
    // create game then...
    // CURRENT_SOLO_GAME.set(createdGameId);
  };
  if (soloGameId) {
    // Subscribe to game websocket
    // Setup game updater
    // -- inc. Robo turn logic - griven by a game input rather than state.
  }
</script>

{#if soloGameBots.length === 0}
  <h2>Building Bots...</h2>
{:else if !soloGameId}
  <button on:click={createSoloGame}>Create Solo Game</button>
{:else}
  <h2>{soloGameId}</h2>
{/if}

<style>
</style>
