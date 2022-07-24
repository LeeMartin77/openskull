import { Button, IconButton, TextField } from "@mui/material";
import { useState } from "react";
import { API_ROOT_URL, USER_ID, USER_ID_HEADER, USER_SECRET, USER_SECRET_HEADER } from "../../../config";
import { CardState, CardType, PlayerGame, RoundPhase, TurnAction } from "../../../models/Game";

import FlowerIcon from '@mui/icons-material/LocalFlorist';
import SkullIcon from '@mui/icons-material/Balcony';
import LostCardIcon from '@mui/icons-material/DoNotDisturbOn';


interface IControlProps { 
  game: PlayerGame, 
  clicked: boolean,
  setClicked: (i: boolean) => void
}

export function GameFlipCardControlComponent({ game, clicked, setClicked }: IControlProps) {
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

  if (game.roundPlayerCardsRevealed[roundIndex][game.activePlayerIndex].length !== game.roundCountPlayerCardsPlayed[roundIndex][game.activePlayerIndex]) {
    return <Button
    variant="contained"
    onClick={() => flipCard(game.activePlayerIndex)}
    disabled={clicked}
    >
      Flip Own Card
    </Button>
  }

  return <>{game.roundCountPlayerCardsPlayed[roundIndex]
    .map((playedCount, i) => [playedCount, i])
    .filter(x => x[1] !== game.activePlayerIndex)
    .map(([playedCount, i]) => {
    return <Button
    key={i}
    variant="contained"
    onClick={() => flipCard(i)}
    disabled={
      clicked ||
      playedCount === game.roundPlayerCardsRevealed[roundIndex][i].length
    }
    >
      Flip {game.playerNicknames[i]}
    </Button>
  })}</>
}