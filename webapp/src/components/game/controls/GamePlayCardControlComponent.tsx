import { IconButton } from "@mui/material";
import { API_ROOT_URL, USER_ID, USER_ID_HEADER, USER_SECRET, USER_SECRET_HEADER } from "../../../config";
import { CardState, CardType, PlayerGame, TurnAction } from "../../../models/Game";

import FlowerIcon from '@mui/icons-material/LocalFlorist';
import SkullIcon from '@mui/icons-material/Balcony';
import LostCardIcon from '@mui/icons-material/DoNotDisturbOn';

interface IControlProps { 
  game: PlayerGame, 
  clicked: boolean,
  setClicked: (i: boolean) => void
}

export function GamePlayCardControlComponent({ game, clicked, setClicked }: IControlProps ) {
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