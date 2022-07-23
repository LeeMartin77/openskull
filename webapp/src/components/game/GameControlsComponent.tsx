import { Button, IconButton, TextField } from "@mui/material";
import { useState } from "react";
import { API_ROOT_URL, USER_ID, USER_ID_HEADER, USER_SECRET, USER_SECRET_HEADER } from "../../config";
import { CardState, CardType, PlayerGame, RoundPhase, TurnAction } from "../../models/Game";

import FlowerIcon from '@mui/icons-material/LocalFlorist';
import SkullIcon from '@mui/icons-material/Balcony';
import LostCardIcon from '@mui/icons-material/DoNotDisturbOn';

const SKIP_VALUE = -1;

interface IControlProps { 
  game: PlayerGame, 
  clicked: boolean,
  setClicked: (i: boolean) => void
}

function GamePlayCardControlComponent({ game, clicked, setClicked }: IControlProps ) {
  const roundIndex = game.roundNumber - 1;

  const playCard = (cardId: string) => {
    setClicked(true);
    fetch(`${API_ROOT_URL}/games/${game.id}/turn`, 
      { 
        method: "POST",
        headers: { "Content-Type": "application/json", [USER_ID_HEADER]: USER_ID, [USER_SECRET_HEADER]: USER_SECRET  },
        body: JSON.stringify({
          action: TurnAction[TurnAction.Card],
          cardId
        })
      })
      // TODO: There is a brief flash of interactivity here...
      .finally(() => setClicked(false))
  }

  return <>{game.playerCards.map(card => {
    if (card.state === CardState.Discarded) {
      return <IconButton size={"large"} key={card.id} color="primary" aria-label="card lost" component="label" disabled>
        <LostCardIcon />
      </IconButton>
    }
    if (card.type === CardType.Flower) {
      return <IconButton size={"large"} color="primary" key={card.id}
        aria-label="play flower" 
        component="label" 
        onClick={() => playCard(card.id)} 
        disabled={clicked || game.activePlayerIndex !== game.playerIndex || game.playerRoundCardIdsPlayed[roundIndex].includes(card.id)}>
        <FlowerIcon />
      </IconButton>
    }
    return <IconButton size={"large"} color="primary" key={card.id}
        aria-label="play skull" 
        component="label" 
        onClick={() => playCard(card.id)} 
        disabled={clicked || game.activePlayerIndex !== game.playerIndex || game.playerRoundCardIdsPlayed[roundIndex].includes(card.id)}>
      <SkullIcon />
    </IconButton>
  })}</>
}

function GamePlaceBidControlComponent({ game, clicked, setClicked }: IControlProps) {
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
      value={bid}
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

function GameFlipCardControlComponent({ game, clicked, setClicked }: IControlProps) {
  const roundIndex = game.roundNumber - 1;

  const flipCard = (targetPlayerIndex: number) => {
    setClicked(true);
    fetch(`${API_ROOT_URL}/games/${game.id}/turn`, 
      { 
        method: "POST",
        headers: { "Content-Type": "application/json", [USER_ID_HEADER]: USER_ID, [USER_SECRET_HEADER]: USER_SECRET  },
        body: JSON.stringify({
          action: TurnAction[TurnAction.Flip],
          targetPlayerIndex
        })
      })
      // TODO: There is a brief flash of interactivity here...
      .finally(() => setClicked(false))
  }

  return <>{game.roundCountPlayerCardsPlayed[roundIndex].map((playedCount, i) => {
    return <Button
    key={i}
    onClick={() => flipCard(i)}
    disabled={
      clicked ||
      (i !== game.activePlayerIndex && game.roundPlayerCardsRevealed[roundIndex][game.activePlayerIndex].length !== playedCount) ||
      playedCount === game.roundPlayerCardsRevealed[roundIndex][i].length
    }
    >
      Flip Player {i}
    </Button>
  })}</>
}

export function GameControlsComponent({ game }: { game: PlayerGame }) {
  
  const [clicked, setClicked] = useState(false);

  if (game.currentRoundPhase !== RoundPhase.Flipping) {
    return <>
      {game.currentRoundPhase !== RoundPhase.Bidding && <div style={{ marginBottom: "1em" }}><GamePlayCardControlComponent game={game} clicked={clicked} setClicked={setClicked}/></div>}
      {game.currentRoundPhase !== RoundPhase.PlayFirstCards && <div><GamePlaceBidControlComponent game={game} clicked={clicked} setClicked={setClicked}/></div>}
    </>
  }
  return <GameFlipCardControlComponent game={game} clicked={clicked} setClicked={setClicked}/>;
}