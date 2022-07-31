<script lang="ts">
  import { navigate } from 'svelte-routing';
  let roomId = '';
  const validRoomId = (aRoomId: string) => {
    let regMatch;
    if (aRoomId) {
      regMatch = roomId.match(/^[a-zA-Z0-9_-]*/);
    }
    return aRoomId && regMatch && regMatch.length === 1 && regMatch[0].length === roomId.length;
  };
</script>

<div>
  <h3>Open Room</h3>
  <input bind:value={roomId} />
  <button on:click={() => navigate(`/rooms/${roomId}`)} disabled={!validRoomId(roomId)}>Go To Room</button>
</div>
