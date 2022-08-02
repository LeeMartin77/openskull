<script lang="ts">
  import type { HubConnection } from '@microsoft/signalr';

  import { OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, playerConnection } from 'src/stores/player';
  import type { OpenskullMessage } from 'src/types/OpenskullMessage';
  import { onDestroy } from 'svelte';
  export let roomId: string;

  let playersInRoom: string[] = [];

  const messageHandler = (msg: OpenskullMessage) => {
    if (msg.activity === 'RoomUpdate' && msg.roomDetails.roomId === roomId) {
      playersInRoom = msg.roomDetails.playerNicknames;
    }
  };

  let connection: HubConnection | undefined = undefined;

  playerConnection.subscribe((con) => {
    con && con.on('send', messageHandler);
    con && con.send('joinRoom', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, roomId);
    connection = con;
    return () => {
      con && con.send('leaveRoom', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, roomId);
      con && con.off('send', messageHandler);
    };
  });

  let startgame = () => {
    connection && connection.send('createRoomGame', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, roomId);
  };

  onDestroy(() => {
    if (connection) {
      connection.send('leaveRoom', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, roomId);
      connection.off('send', messageHandler);
    }
  });
</script>

<div style="max-width: 80vw">
  <h2 style="font-weight: 400">You are in room <span style="font-weight: 700">{roomId}</span></h2>
  <a href="/rooms/{roomId}" class="room-link">Link to Room</a>
  {#if playersInRoom.length < 3}
    <p>Need {3 - playersInRoom.length} more player(s) to start a game.</p>
  {/if}
  {#if playersInRoom.length > 6}
    <p>Max game size of 6 exceeded! Need to lose {playersInRoom.length - 6} players to start game.</p>
  {/if}
  {#if playersInRoom.length > 2 && playersInRoom.length < 7 && connection}
    <div>
      <button class="start-button" on:click={startgame}>Start game!</button>
    </div>
  {/if}
  <h4>Players:</h4>
  <ul>
    {#each playersInRoom as player}
      <li>{player}</li>
    {/each}
  </ul>
</div>

<style>
  .room-link {
    text-align: center;
  }

  .start-button {
    padding: 0.5em;
    width: 100%;
    margin-top: 1em;
    font-weight: 700;
  }
</style>
