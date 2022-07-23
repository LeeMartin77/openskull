import { Button } from "@mui/material";
import { useState } from "react";
import { API_ROOT_URL, USER_ID, USER_ID_HEADER, USER_SECRET, USER_SECRET_HEADER } from "../../config";
import { CardState, CardType, PlayerGame, RoundPhase, TurnAction } from "../../models/Game";

const SKIP_VALUE = -1;

export function GameControlsComponent({ game }: { game: PlayerGame }) {
  const roundIndex = game.roundNumber - 1;
  
  const [clicked, setClicked] = useState(false);

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
      .finally(() => setClicked(false))
  }

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
      .finally(() => setClicked(false))
  }
  
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
      .finally(() => setClicked(false))
  }

  const maxBid = game.roundCountPlayerCardsPlayed[roundIndex].reduce((c, cc) => c + cc, 0);
  const minBid = Math.max(...game.currentBids) + 1;

  const arrayOfValues = [];
  for (let i = minBid; i <= maxBid; i++) {
    arrayOfValues.push(i);
  }

  const cardButtons = game.playerCards.map(card => {
    return <Button
    key={card.id}
    onClick={() => playCard(card.id)}
    color={card.state === CardState.Discarded ? "error" : "primary"}
    disabled={
      ![RoundPhase.PlayFirstCards, RoundPhase.PlayCards].includes(game.currentRoundPhase) || 
      clicked || game.activePlayerIndex !== game.playerIndex || 
      card.state === CardState.Discarded || 
      game.playerRoundCardIdsPlayed[roundIndex].includes(card.id)
    }
    >
      Play {CardType[card.type]}
    </Button>
  })

  const bidControls = <><Button 
      disabled={clicked || game.activePlayerIndex !== game.playerIndex || game.currentRoundPhase === RoundPhase.PlayFirstCards || minBid === 1} 
      onClick={() => placebid(SKIP_VALUE)}>Retire</Button>
    {//This is a completely terrible UI 
    arrayOfValues.map((bidNumber, i) => {
      return <Button key={i}
      disabled={clicked || game.activePlayerIndex !== game.playerIndex  || game.currentRoundPhase === RoundPhase.PlayFirstCards} 
      onClick={() => placebid(bidNumber)}>Bid {bidNumber}</Button>
    })
    }</>
  
  const flipButtons = game.roundCountPlayerCardsPlayed[roundIndex].map((playedCount, i) => {
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
    })

  if (game.currentRoundPhase !== RoundPhase.Flipping) {
    return <>{cardButtons}{bidControls}</>
  }
  return <>{flipButtons}</>;
}