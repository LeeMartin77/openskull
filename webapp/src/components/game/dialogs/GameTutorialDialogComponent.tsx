import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, IconButton } from "@mui/material";
import { useState } from "react";
import HelpOutlineIcon from '@mui/icons-material/HelpOutline';
import { CardDisplayIconsComponent } from "../fragments/CardDisplayIconsComponent";
import { CardType } from "../../../models/Game";

export function GameTutorialDialogComponent() {
  const gameTutorialVersionViewedKey = "Openskull_GameTutorialViewed"
  const currentVersion = "v1.0"
  const [open, setOpen] = useState(localStorage.getItem(gameTutorialVersionViewedKey) !== currentVersion)
  return <>
  <IconButton size={"large"} color="primary"
    aria-label="show tutorial" 
    component="label" 
    onClick={() => setOpen(true)}>
    <HelpOutlineIcon />
  </IconButton>
  <Dialog
    open={open}
    onClose={() => { 
      localStorage.setItem(gameTutorialVersionViewedKey, currentVersion); 
      setOpen(false);
      }}
    aria-labelledby="alert-dialog-title"
    aria-describedby="alert-dialog-description"
  >
    <DialogTitle>
      {"Tutorial"}
    </DialogTitle>
    <DialogContent>
      <ul>
        <li>Skull is played over muliple rounds made up of playing cards, bidding, then flipping played cards.</li>
        <li>To win, you need to successfully reveal the number of cards (starting with your own) you bid, twice.</li>
        <li>If you reveal a skull card however, you lose one of your 4 cards</li>
      </ul>
      <h3>Card Display</h3>
      <DialogContentText>
        The below diplay shows, in order: A revealed flower, an unrevealed, played card, an unplayed card, and a lost Card
      </DialogContentText>
      <CardDisplayIconsComponent {...{ playerRevealedCards: [CardType.Flower], unrevealedCardsPlayed: 1, cardsUnplayed: 1, cardsLost: 1 }}/>
    </DialogContent>
    <DialogActions style={{display: "flex"}}>
      <Button onClick={() => { 
      localStorage.setItem(gameTutorialVersionViewedKey, currentVersion); 
      setOpen(false);
      }}>Dismiss</Button>
    </DialogActions>
  </Dialog>
</>
}