<script lang="ts">
  import { OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, playerConnection } from "src/stores/player";
import type { OpenskullMessage } from "src/types/OpenskullMessage";
  export let roomId: string;

  let playersInRoom: string[] = [];

  const messageHandler = (msg: OpenskullMessage) => {
    if (msg.activity === "RoomUpdate" && msg.roomDetails.roomId === roomId) {
      playersInRoom = msg.roomDetails.playerNicknames;
    }
  }

  playerConnection.subscribe(con => {
    con && con.on("send", messageHandler)
    con && con.send("joinRoom", OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, roomId)
    return () => {
      con && con.send("leaveRoom", OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, roomId)
      con && con.off("send", messageHandler)
    }
  })
</script>
<div>
  <h2>You are in room {roomId}</h2>
  <h4>Players:</h4>
  <ul>
    {#each playersInRoom as player}
      <li>{player}</li>
    {/each}
  </ul>
</div>