import { Button, TextField } from "@mui/material";
import { useState } from "react";
import { API_ROOT_URL, USER_ID, USER_ID_HEADER, USER_SECRET, USER_SECRET_HEADER } from "../../../config";
import { PlayerGame, TurnAction } from "../../../models/Game";

const SKIP_VALUE = -1;

interface IControlProps { 
  game: PlayerGame, 
  clicked: boolean,
  setClicked: (i: boolean) => void
}

export function GamePlaceBidControlComponent({ game, clicked, setClicked }: IControlProps) {
  const roundIndex = game.roundNumber - 1;
  const minBid = Math.max(...game.currentBids) + 1;

  const [bid, setBid] = useState(minBid);

  const placebid = (bid: number) => {
    setClicked(true);
    fetch(`${API_ROOT_URL}/games/${game.id}/turn`, 
      { 
        method: "POST",
        headers: { "Content-Type": "application/json", [USER_ID_HEADER]: USER_ID, [USER_SECRET_HEADER]: USER_SECRET  },
        body: JSON.stringify({
          action: TurnAction[TurnAction.Bid],
          bid
        })
      })
      // TODO: There is a brief flash of interactivity here...
      .finally(() => setClicked(false))
  }

  const maxBid = game.roundCountPlayerCardsPlayed[roundIndex].reduce((c, cc) => c + cc, 0);

  return <>
    {minBid < maxBid && <TextField
      id="number-bid"
      label="Bid"
      type="number"
      value={bid > minBid ? bid : minBid}
      onChange={(e) => setBid(parseInt(e.target.value))}
      InputProps={{ 
        inputProps: {
          min: minBid,
          max: maxBid
        },
        endAdornment: <Button variant="contained"
        disabled={clicked || game.activePlayerIndex !== game.playerIndex} 
        onClick={() => placebid(bid)}>Bid</Button>
      }}
      InputLabelProps={{
        shrink: true,
      }}
    />}
    {minBid > 1 && <div style={{ marginTop: "1em" }} ><Button variant="contained"
    disabled={clicked || game.activePlayerIndex !== game.playerIndex} 
    onClick={() => placebid(SKIP_VALUE)}>Withdraw</Button></div>}
  </>
}