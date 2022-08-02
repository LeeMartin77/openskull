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

<RoomJoinInterface
  on:navigated={({ detail }) => {
    dispatchNavigated(detail.to);
  }}
/>
<UpdatePlayerNickname showDummyButton={true} />

<style>
  .navigation-button {
    border: none;
    width: 100%;
    font-weight: 700;
    padding: 0.5em;
    margin: 0.5em 0;
  }

  .navigation-button:hover {
    background-color: lightgray;
  }
</style>
