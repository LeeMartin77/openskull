import { Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField } from "@mui/material";
import { useState } from "react";
import { useNavigate } from "react-router-dom";

export function PlayerRoomDialogComponent({ open, setOpen }: { open: boolean, setOpen: (i: boolean) => void }) {
  const navigate = useNavigate();
  const [roomId, setRoomId] = useState("");

  const regMatch = roomId.match(/^[a-zA-Z0-9_-]*/)
  const validRoomId = roomId.length > 0 && 
    regMatch && 
    regMatch.length === 1 && 
    regMatch[0].length === roomId.length

  return <Dialog
    open={open}
    onClose={() => setOpen(false)}
    aria-labelledby="alert-dialog-title"
    aria-describedby="alert-dialog-description"
  >
  <DialogTitle>
    {"Go to Room"}
  </DialogTitle>
  <DialogContent>
    <TextField
      id="room-id"
      label="Room Id"
      value={roomId}
      error={!validRoomId && roomId.length > 0}
      helperText={"Must match a-z, A-Z, 0-9, _ and - (no spaces!)"}
      onChange={(e) => setRoomId(e.target.value)}
    />
  </DialogContent>
  <DialogActions style={{display: "flex"}}>
    <Button onClick={() => setOpen(false)}>Cancel</Button>
    <Button onClick={() => {
      navigate("/rooms/" + roomId)
      setOpen(false)
    }} variant="contained" disabled={!validRoomId}>
      Open Room
    </Button>
  </DialogActions>
</Dialog>
}