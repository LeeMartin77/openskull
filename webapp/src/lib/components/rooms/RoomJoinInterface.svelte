<script>
  import { Link } from 'svelte-routing';
  let roomId = '';
</script>

<div>
  <input bind:value={roomId} />
  {#if roomId && roomId.length > 0}
    <Link to="/rooms/{roomId}">Go To Room</Link>
  {/if}
</div>
