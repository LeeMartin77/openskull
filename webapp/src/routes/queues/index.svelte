<script lang="ts">
  import { onDestroy } from 'svelte';

  import { OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, playerConnection } from 'src/stores/player';
  import type { OpenskullMessage } from 'src/types/OpenskullMessage';
  import type { HubConnection } from '@microsoft/signalr';

  type QueueStatus = {
    gameSize: number;
    queueSize: number;
  };

  let queueStatus: QueueStatus | undefined = undefined;
  let connection: HubConnection;
  let queueSizes: number[] = [3, 4, 5, 6];

  const messageHandler = (msg: OpenskullMessage) => {
    if (msg.activity === 'GameCreated') {
      connection.send('getQueueStatus', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET);
    }
    if (msg.activity === 'QueueLeft') {
      queueStatus = undefined;
    }
    if (msg.activity === 'QueueStatus' || msg.activity === 'QueueJoined') {
      if (msg.details.gameSize === 0) {
        queueStatus = undefined;
      } else {
        queueStatus = msg.details;
      }
    }
  };

  playerConnection.subscribe((con) => {
    connection = con;
    let interval: NodeJS.Timer;
    if (con) {
      con.on('send', messageHandler);
      con.send('getQueueStatus', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET);
      // we should probably make the API ping out when queue changes
      interval = setInterval(() => {
        con.send('getQueueStatus', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET);
      }, 500);
    }
    return () => {
      if (con) {
        interval && clearInterval(interval);
        con.send('LeaveQueues', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET);
        con.off('send', messageHandler);
      }
    };
  });

  onDestroy(() => {
    if (connection) {
      connection.send('LeaveQueues', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET);
      connection.off('send', messageHandler);
    }
  });
</script>

<div>
  {#if connection}
    <h3 style="text-align: center">Game Queues</h3>
    <div class="button-container">
      {#if queueStatus === undefined}
        {#each queueSizes as size}
          <button
            class="queue-button"
            on:click={() => connection.send('JoinQueue', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, size)}
            >Join {size} Player Queue</button
          >
        {/each}
      {:else}
        <div style="margin-bottom: 2em;">
          In {queueStatus.gameSize} player game queue, awaiting {queueStatus.gameSize - queueStatus.queueSize} players
        </div>
        <button
          class="queue-button"
          on:click={() => connection.send('LeaveQueues', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET)}>Leave Queue</button
        >
      {/if}
    </div>
  {:else}
    Connecting...
  {/if}
</div>

<style>
  .button-container {
    width: 100%;
    display: flex;
    flex-direction: column;
    align-items: center;
  }
  .queue-button {
    width: 70%;
    padding: 0.5em;
    font-weight: 700;
    font-size: 1em;
    margin: 0.25em 0;
  }
</style>
