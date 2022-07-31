<script lang="ts">
  import {
    playerConnection,
    OPENSKULL_USER_NICKNAME,
    OPENSKULL_USER_ID,
    OPENSKULL_USER_SECRET
  } from 'src/stores/player';

  export let showDummyButton: boolean = false;

  function debounce(func, timeout = 300) {
    let timer;
    return (...args) => {
      clearTimeout(timer);
      timer = setTimeout(() => {
        func.apply(this, args);
      }, timeout);
    };
  }
</script>

<div>
  <h3>Update Nickname</h3>
  <input
    bind:value={$OPENSKULL_USER_NICKNAME}
    on:change={(e) => {
      if (e.currentTarget.value) {
        debounce(
          $playerConnection.send('UpdateNickname', OPENSKULL_USER_ID, OPENSKULL_USER_SECRET, e.currentTarget.value)
        );
      }
    }}
  />
  {#if showDummyButton}
    <button>Update</button>
  {/if}
</div>
