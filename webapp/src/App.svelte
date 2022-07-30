<script lang="ts">
  import { Router, Link, Route } from "svelte-routing";
  import GamesView from "./routes/games/[id].svelte";
  import GamesList from "./routes/games/index.svelte";
  import RoomView from "./routes/rooms/[roomId].svelte"
  import QueuesView from "./routes/queues/index.svelte";
  import Home from "./routes/index.svelte";
  import PlayerConnectionToaster from "./lib/components/toasters/PlayerConnectionToaster.svelte";
  import GameCreatedDialog from "./lib/components/game/GameCreatedDialog.svelte";
  import RoomJoinDialog from "./lib/components/rooms/RoomJoinDialog.svelte";
  export let url = "";
</script>

<main>
  <Router url="{url}">
    <nav>
      <Link to="/">Home</Link>
      <Link to="games">Games</Link>
      <Link to="queues">Queues</Link>
    </nav>
    <RoomJoinDialog />
    <div>
      <Route path="rooms/:roomId" component="{RoomView}"/>
      <Route path="games/:id" component="{GamesView}" />
      <Route path="games" component="{GamesList}" />
      <Route path="queues" component="{QueuesView}" />
      <Route path="/"><Home /></Route>
    </div>
  </Router>
  <GameCreatedDialog />
  <PlayerConnectionToaster />
</main>