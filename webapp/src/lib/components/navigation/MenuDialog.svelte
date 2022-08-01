<script lang="ts">
  import Dialog from 'src/lib/components/dialog/Dialog.svelte';
  import Menu from 'src/lib/components/navigation/Menu.svelte';

  let open = false;
</script>

<button
  class="menu-button"
  on:click={() => {
    open = false;
    open = true;
  }}>Menu</button
>
<Dialog {open}>
  <Menu />
</Dialog>

<style>
  .menu-button {
    position: fixed;
    bottom: 1em;
    left: 50%;
    transform: translate(-50%, 0);
    padding: 0.5em;
    font-weight: 700;
  }
</style>