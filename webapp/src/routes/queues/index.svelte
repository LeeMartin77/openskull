<script lang="ts">
  
  import { onDestroy } from "svelte";

  import { OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, playerConnection } from "src/stores/player"
  import type { OpenskullMessage } from "src/types/OpenskullMessage";
  import type { HubConnection } from "@microsoft/signalr";

  type QueueStatus = {
    gameSize: number,
    queueSize: number
  }

  let queueStatus: QueueStatus | undefined = undefined;
  let connection: HubConnection;
  let queueSizes: number[] = [3, 4, 5, 6]

  const messageHandler = (msg: OpenskullMessage) => {
    if (msg.activity === "GameCreated") {
        connection.send("getQueueStatus", OPENSKULL_USER_ID, OPENSKULL_USER_SECRET)
      }
      if (msg.activity === "QueueLeft") {
        queueStatus = undefined;
      }
      if (msg.activity === "QueueStatus" || msg.activity === "QueueJoined") {
        if (msg.details.gameSize === 0) {
          queueStatus = undefined;
        } else {
          queueStatus = msg.details;
        }
      }
  }

  playerConnection.subscribe(con => {
    connection = con;
    let interval: NodeJS.Timer;
    if (con) {
      con.on("send", messageHandler)
      con.send("getQueueStatus", OPENSKULL_USER_ID, OPENSKULL_USER_SECRET); 
      // we should probably make the API ping out when queue changes
      interval = setInterval(() => {
        con.send("getQueueStatus", OPENSKULL_USER_ID, OPENSKULL_USER_SECRET); 
      }, 500)
    }
    return () => { if(con) { 
      interval && clearInterval(interval);
      con.send("LeaveQueues", OPENSKULL_USER_ID, OPENSKULL_USER_SECRET); 
      con.off("send", messageHandler);
    } };
  })

  onDestroy(() => { 
    if (connection) {
      connection.send("LeaveQueues", OPENSKULL_USER_ID, OPENSKULL_USER_SECRET); 
      connection.off("send", messageHandler);
    }
  })
</script>

<div>
  {#if connection}
    {#if queueStatus === undefined}
    <ul>
      {#each queueSizes as size}
        <li><button on:click={() => connection.send("JoinQueue", OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, size)}>Join {size} Queue</button></li>
      {/each}
    </ul>
    {:else}
      <div>
        In {queueStatus.gameSize} player game queue, with {queueStatus.queueSize} players
      </div>
      <button on:click={() => connection.send("LeaveQueues", OPENSKULL_USER_ID, OPENSKULL_USER_SECRET)}>Leave Queues</button>
    {/if}
  {:else}
    Connecting...
  {/if}
</div>
