<script lang="ts">
  import Dialog from 'src/lib/components/dialog/Dialog.svelte';
  import Menu from 'src/lib/components/navigation/Menu.svelte';

  let open = false;
</script>

<button
  on:click={() => {
    open = false;
    open = true;
  }}>Menu</button
>
<Dialog {open}>
  <Menu />
</Dialog>
