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

<main>
  <Router {url}>
    <div>
      <Route path="rooms/:roomId" component={RoomView} />
      <Route path="games/:gameId" component={GamesView} />
      <Route path="games" component={GamesList} />
      <Route path="queues" component={QueuesView} />
      <Route path="/" component={Menu} />
      <Route path="*" component={NotFound} />
    </div>
    <GameCreatedDialog />
    <NavigationMenu />
  </Router>
  <NewPlayerNicknameDialog />
  <PlayerConnectionToaster />
</main>
