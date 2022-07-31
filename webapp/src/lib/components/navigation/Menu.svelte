<script lang="ts">
  import { Link } from 'svelte-routing';
  import RoomJoinInterface from 'src/lib/components/rooms/RoomJoinInterface.svelte';
  import UpdatePlayerNickname from '../player/UpdatePlayerNickname.svelte';
</script>

<div>
  <Link to="games">Games</Link>
</div>
<div>
  <Link to="queues">Queues</Link>
</div>
<RoomJoinInterface />
<UpdatePlayerNickname showDummyButton={true} />
