<script lang="ts">
  import { Router, Link, Route } from 'svelte-routing';
  import GamesView from './routes/games/[gameId].svelte';
  import GamesList from './routes/games/index.svelte';
  import RoomView from './routes/rooms/[roomId].svelte';
  import QueuesView from './routes/queues/index.svelte';
  import NotFound from './routes/NotFound.svelte';
  import PlayerConnectionToaster from './lib/components/toasters/PlayerConnectionToaster.svelte';
  import GameCreatedDialog from './lib/components/game/GameCreatedDialog.svelte';
  import NavigationMenu from './lib/components/navigation/MenuDialog.svelte';
  import Menu from './lib/components/navigation/Menu.svelte';
  import NewPlayerNicknameDialog from './lib/components/player/NewPlayerNicknameDialog.svelte';
  export let url = '';
</script>

<main class="main">
  <Router {url}>
    <div class="container">
      <div>
        <Route path="rooms/:roomId" component={RoomView} />
        <Route path="games/:gameId" component={GamesView} />
        <Route path="games" component={GamesList} />
        <Route path="queues" component={QueuesView} />
        <Route path="/" component={Menu} />
        <Route path="*" component={NotFound} />
      </div>
    </div>
    <GameCreatedDialog />
    <NavigationMenu />
  </Router>
  <NewPlayerNicknameDialog />
  <PlayerConnectionToaster />
</main>

<style>
  .main {
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    position: fixed;
  }
  .container {
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    display: flex;
    justify-content: center;
    position: fixed;
    overflow-y: auto;
  }
</style>