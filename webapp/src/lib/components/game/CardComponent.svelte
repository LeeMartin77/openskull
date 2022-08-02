<script lang="ts">
  export let onClick: () => void | undefined = undefined;
  export let display: string;
  export let disabled: boolean = false;
  export let lost: boolean = false;
  export let scale: number = 1;
</script>

<button
  on:click={() => {
    if (onClick && !disabled && !lost) {
      onClick();
    }
  }}
  disabled={disabled || lost}
  style="--theme-scale: {scale}"
  class="card card-{display.toLowerCase()} {lost
    ? 'card-lost'
    : disabled
    ? 'card-disabled'
    : onClick
    ? 'card-active'
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
    width: calc(8em * var(--theme-scale));
    height: calc(12em * var(--theme-scale));
    border-radius: calc(1em * var(--theme-scale));
    margin: calc(0.25em * var(--theme-scale));
    background-color: #fff;
    border-color: #000;
    border-width: 2px;
    border-style: solid;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .card-active:hover {
    background-color: #ddd;
  }

  .card img {
    width: 75%;
  }

  .card-lost {
    filter: opacity(10%);
  }

  .card-disabled {
    filter: opacity(40%);
  }

  .card-back {
    background-color: rgb(145, 145, 145);
  }
</style>
