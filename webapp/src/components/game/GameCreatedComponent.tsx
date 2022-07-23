import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Link } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { Link as LinkIcon } from "@mui/icons-material"

interface GameCreatedModalComponentProps { 
  gameId: string | undefined, 
  handleDismiss: () => void
}

export function GameCreatedModalComponent(
  { gameId, handleDismiss } : GameCreatedModalComponentProps
  ): JSX.Element {
  const navigate = useNavigate();
  return !gameId ? <></> : <Dialog
  open={gameId !== undefined}
  onClose={handleDismiss}
  aria-labelledby="alert-dialog-title"
  aria-describedby="alert-dialog-description"
>
  <DialogTitle>
    {"A Game Has Been Started!"}
  </DialogTitle>
  <DialogContent>
    <DialogContentText>
      Click "Go To Game" below to go to the game, or use "Link" to open the game in a new tab.
    </DialogContentText>
  </DialogContent>
  <DialogActions style={{display: "flex"}}>
    <Button 
      component={Link} 
      href={"/games/" + gameId} 
      color="secondary" 
      variant="outlined"
      style={{marginLeft: '0', marginRight: 'auto'}}
      endIcon={<LinkIcon />}
      >Link</Button>
    <Button onClick={handleDismiss}>Ignore</Button>
    <Button onClick={() => {
      navigate("/games/" + gameId) 
      handleDismiss()
    }} variant="contained" autoFocus>
      Go To Game
    </Button>
  </DialogActions>
</Dialog>
}