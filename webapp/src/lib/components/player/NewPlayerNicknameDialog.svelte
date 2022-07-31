<script lang="ts">
  import Dialog from 'src/lib/components/dialog/Dialog.svelte';
  import { OPENSKULL_USER_NICKNAME, DEFAULT_USER_NICKNAME } from 'src/stores/player';
  import UpdatePlayerNickname from './UpdatePlayerNickname.svelte';
  let startOpen = false;
  OPENSKULL_USER_NICKNAME.subscribe((nick) => {
    if (!startOpen && (!nick || nick === DEFAULT_USER_NICKNAME)) {
      startOpen = true;
    }
  });
</script>

<Dialog open={startOpen} complete={{ text: 'Submit', action: () => {} }}>
  <div>
    <h2>Welcome! Please set a nickname</h2>
  </div>
  <UpdatePlayerNickname />
</Dialog>
