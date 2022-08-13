<script lang="ts">
  import { navigate } from 'svelte-routing';
  import { createEventDispatcher } from 'svelte';
  import RoomJoinInterface from 'src/lib/components/rooms/RoomJoinInterface.svelte';
  import UpdatePlayerNickname from '../player/UpdatePlayerNickname.svelte';

  const dispatch = createEventDispatcher();
  const dispatchNavigated = (to: string) => {
    dispatch('navigated', { to });
  };
</script>

<div class="navigation-buttons">
  <a href="https://www.openskull.dev" class="navigation-button">About</a>

  <button
    class="navigation-button"
    on:click={() => {
      navigate('/games');
      dispatchNavigated('games');
    }}>Games List</button
  >
  <button
    class="navigation-button"
    on:click={() => {
      navigate('/queues');
      dispatchNavigated('queues');
    }}>Game Queues</button
  >
</div>

<RoomJoinInterface
  on:navigated={({ detail }) => {
    dispatchNavigated(detail.to);
  }}
/>
<UpdatePlayerNickname showDummyButton={true} />

<style>
  .navigation-buttons {
    display: flex;
    flex-direction: column;
    align-items: stretch;
  }

  .navigation-button {
    border: none;
    font-weight: 400;
    line-height: 1em;
    padding: 0.5em;
    margin: 0.5em 0;
    background-color: #eee;
    flex: 1;
    color: black;
    text-decoration: none;
    text-align: center;
    font-size: 14px;
    cursor: pointer;
  }

  .navigation-button:hover {
    background-color: lightgray;
  }
</style>
