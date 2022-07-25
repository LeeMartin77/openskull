import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";

import { PlayerGame, PublicGame } from "../../../models/Game";

export interface IGameCompleteDialogProps { game: PublicGame | PlayerGame, open: boolean }

export function GameCompleteDialogComponent(
  { game, open, setGameCompleteDialog }: IGameCompleteDialogProps & { setGameCompleteDialog : (i: IGameCompleteDialogProps) => void}
  ) {
  const numberOfRounds = game.roundCountPlayerCardsPlayed.length
  const winner = game.playerNicknames[game.activePlayerIndex]
  return <Dialog
  open={open}
  onClose={() => setGameCompleteDialog({ game, open: false })}
  aria-labelledby="alert-dialog-title"
  aria-describedby="alert-dialog-description"
>
  <DialogTitle>
    {"Game Complete!"}
  </DialogTitle>
  <DialogContent>
    <DialogContentText>
      {winner} won after {numberOfRounds} rounds
    </DialogContentText>
  </DialogContent>
  <DialogActions style={{display: "flex"}}>
    <Button onClick={() => setGameCompleteDialog({ game, open: false })}>Dismiss</Button>
  </DialogActions>
</Dialog>
}