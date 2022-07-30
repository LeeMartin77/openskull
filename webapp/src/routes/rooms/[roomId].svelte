<script lang="ts">
  import type { HubConnection } from '@microsoft/signalr';

  import {
    OPENSKULL_USER_ID,
    OPENSKULL_USER_SECRET,
    playerConnection
  } from 'src/stores/player';
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
    con &&
      con.send('joinRoom', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, roomId);
    connection = con;
    return () => {
      con &&
        con.send('leaveRoom', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, roomId);
      con && con.off('send', messageHandler);
    };
  });

  let startgame = () => {
    connection &&
      connection.send(
        'createRoomGame',
        OPENSKULL_USER_ID,
        OPENSKULL_USER_SECRET,
        roomId
      );
  };

  onDestroy(() => {
    if (connection) {
      connection.send(
        'leaveRoom',
        OPENSKULL_USER_ID,
        OPENSKULL_USER_SECRET,
        roomId
      );
      connection.off('send', messageHandler);
    }
  });
</script>

<div>
  <h2>You are in room {roomId}</h2>
  <h4>Players:</h4>
  {#if playersInRoom.length > 2 && playersInRoom.length < 7 && connection}
    <button on:click={startgame}>Start game!</button>
  {/if}
  <ul>
    {#each playersInRoom as player}
      <li>{player}</li>
    {/each}
  </ul>
</div>
