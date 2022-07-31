<script lang="ts">
  import { Link } from 'svelte-routing';
  import RoomJoinInterface from 'src/lib/components/rooms/RoomJoinInterface.svelte';
</script>

<Link to="games">Games</Link>
<Link to="queues">Queues</Link>
<RoomJoinInterface />
