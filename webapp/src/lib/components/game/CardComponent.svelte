<script lang="ts">
  export let onClick: () => void | undefined = undefined;
  export let display: string;
  export let disabled: boolean = false;
  export let lost: boolean = false;
</script>

<button
  on:click={() => {
    if (onClick && !disabled && !lost) {
      onClick();
    }
  }}
  disabled={disabled || lost}
  class="card card-{display.toLowerCase()} {lost
    ? 'card-lost'
    : disabled
    ? 'card-disabled'
    : ''}"
>
  {#if display === 'Skull'}
    <img src="/openskull.svg" alt="A skull" />
  {:else if display === 'Flower'}
    <img src="/flower.svg" alt="A flower" />
  {:else}
    <span>Back</span>
  {/if}
</button>

<style>
  .card {
    width: 8em;
    height: 12em;
    border-radius: 1em;
    background-color: #fff;
    border-color: #000;
    border-width: 2px;
    border-style: solid;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .card img {
    width: 75%;
  }

  .card-lost {
    width: 6em;
    height: 9em;
    filter: opacity(40%)
  }

  .card-disabled {
    filter: opacity(40%) 
  }

  .card-back {
    background-color: rgb(145, 145, 145);
  }
</style>
