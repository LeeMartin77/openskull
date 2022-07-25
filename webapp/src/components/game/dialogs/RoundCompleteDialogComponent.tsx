import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";

import { PlayerGame, PublicGame } from "../../../models/Game";


export interface IRoundCompleteProps { prevGame: PublicGame | PlayerGame, game: PublicGame | PlayerGame, open: boolean }

export function RoundCompleteDialogComponent(
  { prevGame, game, open, setRoundChangeDialog }: IRoundCompleteProps & { setRoundChangeDialog : (i: IRoundCompleteProps) => void}
  ) {

  const roundNumber = prevGame.roundNumber
  const flipper = prevGame.playerNicknames[prevGame.activePlayerIndex]
  const roundWon = game.roundWinners.filter(x => x === prevGame.activePlayerIndex).length > prevGame.roundWinners.filter(x => x === prevGame.activePlayerIndex).length
  return <Dialog
  open={open}
  onClose={() => setRoundChangeDialog({ prevGame, game, open: false })}
  aria-labelledby="alert-dialog-title"
  aria-describedby="alert-dialog-description"
>
  <DialogTitle>
    {"Round "+roundNumber+" Finished!"}
  </DialogTitle>
  <DialogContent>
    <DialogContentText>
      Round {roundNumber} finished when {flipper} stopped revealing cards, {roundWon ? "winning" : "losing"} the round. 
    </DialogContentText>
  </DialogContent>
  <DialogActions style={{display: "flex"}}>
    <Button onClick={() => setRoundChangeDialog({ prevGame, game, open: false })}>Dismiss</Button>
  </DialogActions>
</Dialog>
}