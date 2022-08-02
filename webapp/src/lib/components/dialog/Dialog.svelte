<script lang="ts">
  export let open: boolean = false;
  export let complete: { text: string; action: () => void } | undefined = undefined;
  export let hideClose: boolean = false;
  const dismiss = () => (open = false);
  const dismissComplete = (fnr: () => void) => {
    fnr();
    open = false;
  };
</script>

{#if open}
  <div class="lightbox">
    <div class="dialog">
      <div class="dialog-bar">
        {#if !hideClose}<button class="close-button" on:click={() => dismiss()}>Close</button>{/if}
      </div>
      <div class="dialog-content">
        <slot />
      </div>
      {#if complete}
        <div class="dialog-bar"><button on:click={() => dismissComplete(complete.action)}>{complete.text}</button></div>
      {/if}
    </div>
  </div>
{/if}

<style>
  .lightbox {
    position: fixed;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
    background-color: rgba(255, 255, 255, 0.8);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 999;
  }
  .dialog {
    background-color: #fff;
    border-radius: 1em;
    box-shadow: 0.25em 0.25em 0.5em 0em rgba(0, 0, 0, 0.2);
    min-width: 160px;
    min-height: 160px;
    max-width: 90vw;
  }
  .dialog-bar {
    display: flex;
    border-radius: 1em;
    padding: 0.5em;
    width: calc(100% - 1em);
  }
  .close-button {
    margin-left: auto;
    margin-right: 0.5em;
    padding: 0.5em;
    font-weight: 700;
  }
  .dialog-content {
    padding: 1em;
    width: calc(100% - 2em);
  }
</style>
