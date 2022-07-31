<script lang="ts">
  export let open: boolean = false;
  export let complete: { text: string, action: () => void } | undefined = undefined;
  const dismiss = () => open = false;
  const dismissComplete = (fnr: () => void) => { fnr(); open = false; }

</script>
{#if open}
<div class="lightbox" on:click={dismiss}>
  <div class="dialog">
    <div class="dialog-bar"><button on:click={() => dismiss()}>Close</button></div>
    <div class="dialog-content">
      <slot></slot>
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
  }
  .dialog {
    background-color: #fff;
    border-radius: 1em;
    box-shadow: 0.25em 0.25em 0.5em 0em rgba(0,0,0,0.2);
    min-width: 160px;
    min-height: 160px;
  }
  .dialog-bar {
    border-radius: 1em;
    padding: 0.5em;
    width: 100%;
  }
  .dialog-content {
    padding: 1em;
    width: 100%;
  }
</style>