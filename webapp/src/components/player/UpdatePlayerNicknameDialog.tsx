import { HubConnection } from "@microsoft/signalr";
import { Button, Dialog, DialogActions, DialogContent, DialogTitle, IconButton, TextField } from "@mui/material";
import { useState } from "react";
import { USER_ID, USER_NICKNAME_IDENTIFIER, USER_SECRET } from "../../config";
import ManageAccountsIcon from '@mui/icons-material/ManageAccounts';

interface IUpdateUserNicknameDialogComponent {
  playerNickname: string,
  updatePlayerNickname: (n: string) => void,
  playerConnection: HubConnection
}

export function UpdateUserProfileButton({ 
  playerNickname,
  updatePlayerNickname,
  playerConnection
}: IUpdateUserNicknameDialogComponent) {
  const [open, setOpen] = useState(false);
  return <>
  <IconButton 
  onClick={() => setOpen(true)}
  style={{ position: "fixed", top: "1rem", right: "1rem" }}
  aria-label="manage profile">
    <ManageAccountsIcon />
  </IconButton>
  <UpdateUserNicknameDialogComponent
    playerConnection={playerConnection}
    playerNickname={playerNickname}
    updatePlayerNickname={updatePlayerNickname}
    open={open}
    setOpen={setOpen}
  />
  </>
}

export function UpdateUserNicknameDialogComponent({ 
    playerNickname,
    updatePlayerNickname,
    playerConnection,
    open,
    setOpen
  }: IUpdateUserNicknameDialogComponent & { open: boolean, setOpen: (i: boolean) => void }) {
  const [editorNickname, setEditorNickname] = useState(playerNickname);

  const updateNickname = (newNickname: string) => {
    playerConnection.send("updateNickname", USER_ID, USER_SECRET, newNickname)
    localStorage.setItem(USER_NICKNAME_IDENTIFIER, newNickname)
    updatePlayerNickname(newNickname)
  }
  return <Dialog
  open={open}
  onClose={() => setOpen(false)}
  aria-labelledby="alert-dialog-title"
  aria-describedby="alert-dialog-description"
>
  <DialogTitle>
    {"Update Nickname"}
  </DialogTitle>
  <DialogContent>
    <TextField
      id="player-nickname"
      label="Nickname"
      value={editorNickname}
      onChange={(e) => setEditorNickname(e.target.value)}
    />
  </DialogContent>
  <DialogActions style={{display: "flex"}}>
    <Button onClick={() => setOpen(false)}>Dismiss</Button>
    <Button onClick={() => {
      updateNickname(editorNickname)
      setOpen(false)
    }} variant="contained" autoFocus>
      Update
    </Button>
  </DialogActions>
</Dialog>
}