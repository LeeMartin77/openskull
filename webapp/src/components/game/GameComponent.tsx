import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { Alert, Button, Card, CardContent, CardHeader, CircularProgress, List, ListItem } from "@mui/material";
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { API_ROOT_URL, USER_ID, USER_ID_HEADER, USER_SECRET, USER_SECRET_HEADER } from "../../config";
import { CardState, CardType, PlayerGame, PublicGame, RoundPhase, TurnAction } from "../../models/Game";

import FlowerIcon from '@mui/icons-material/LocalFlorist';
import LostCardIcon from '@mui/icons-material/DoNotDisturbOn';
import CircleIcon from '@mui/icons-material/Circle';
import SkullIcon from '@mui/icons-material/Balcony';

const SKIP_VALUE = -1;

const updateGame = (
  gameId: string, 
  fnSetGame: React.Dispatch<React.SetStateAction<PlayerGame | PublicGame | undefined>>,
  fnSetError: React.Dispatch<React.SetStateAction<boolean>>, 
  fnSetLoading: React.Dispatch<React.SetStateAction<boolean>>
  ) => {
  fnSetLoading(true);
  return fetch(`${API_ROOT_URL}/games/${gameId}`, 
  { headers: { "Content-Type": "application/json", [USER_ID_HEADER]: USER_ID, [USER_SECRET_HEADER]: USER_SECRET }})
  .then(async res => {
    const item = await res.json();
    fnSetGame(item);
  })
  .catch(() => fnSetError(true))
  .finally(() => fnSetLoading(false))
}

function PublicPlayerView({ index, game }: { index: number, game: PublicGame }) {
  const roundIndex = game.roundNumber - 1;
  const wins = game.roundWinners.filter(x => x === index).length;
  const playerRevealedCards = game.roundPlayerCardsRevealed[roundIndex][index].map(card => card === CardType.Flower ? <FlowerIcon /> : <SkullIcon />);
  const playedRevealedCardsCount = game.roundPlayerCardsRevealed[roundIndex][index].length;
  const unrevealedCardsPlayed = game.roundCountPlayerCardsPlayed[roundIndex][index] - playedRevealedCardsCount;
  const cardsUnplayed = game.currentCountPlayerCardsAvailable[index] - game.roundCountPlayerCardsPlayed[roundIndex][index];
  const cardsLost = game.playerCardStartingCount - game.currentCountPlayerCardsAvailable[index];
  const currentBid = game.currentBids[index] === SKIP_VALUE ? "Withdrawn" : game.currentBids[index] === 0 ? "No Bid" : game.currentBids[index]
  //TODO: We need to make games return nicknames for nicer identification
  return <Card variant={game.activePlayerIndex === index ? "outlined" : undefined}>
    <CardHeader title={"Player " + index} subheader={wins + " point(s)"}></CardHeader>
    <CardContent>
      <div>
        {playerRevealedCards}
        {Array.from(Array(unrevealedCardsPlayed).keys()).map(() => <CircleIcon />)}
        {Array.from(Array(cardsUnplayed).keys()).map(() => <CircleIcon color="disabled" />)}
        {Array.from(Array(cardsLost).keys()).map(() => <LostCardIcon color="disabled" />)}
      </div>
      {(game.currentRoundPhase == RoundPhase.Bidding || game.currentRoundPhase == RoundPhase.Flipping && game.activePlayerIndex === index) && <div>
        Bid: {currentBid}
      </div>}
    </CardContent>
  </Card>
}

function GameControlsComponent({ game }: { game: PlayerGame }) {
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

  return <Card variant={'elevation'}>
  <CardHeader title={"Controls"}/>
  <CardContent><List>
  <ListItem key="available">Cards Available: {game.currentCountPlayerCardsAvailable[game.playerIndex]}/{game.playerCardStartingCount}</ListItem>
  <ListItem key="played">Cards Played: {game.roundCountPlayerCardsPlayed[roundIndex][game.playerIndex]}</ListItem>
  <ListItem key="bid">Current Bid: {game.currentBids[game.playerIndex] === SKIP_VALUE ? "Withdrawn" : game.currentBids[game.playerIndex] }</ListItem>
  <ListItem key="revealed">Cards Revealed: {game.roundPlayerCardsRevealed[roundIndex][game.playerIndex].length}: {game.roundPlayerCardsRevealed[roundIndex][game.playerIndex].map(card => <> {CardType[card]} </>) }</ListItem>
  <ListItem key="wins">Wins: {game.roundWinners.filter(x => x === game.playerIndex).length}</ListItem>
  {!game.gameComplete && game.currentRoundPhase !== RoundPhase.Flipping && 
  <ListItem key="play-card">
    {game.playerCards.map(card => {
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
    })}
  </ListItem>}
  {!game.gameComplete && (game.currentRoundPhase !== RoundPhase.Flipping) && 
  <ListItem key="place-bid">
    <Button 
      disabled={clicked || game.activePlayerIndex !== game.playerIndex || game.currentRoundPhase === RoundPhase.PlayFirstCards || minBid === 1} 
      onClick={() => placebid(SKIP_VALUE)}>Retire</Button>
    {//This is a completely terrible UI  clicked || game.activePlayerIndex !== game.playerIndex
    arrayOfValues.map(bidNumber => {
      return <Button 
      disabled={clicked || game.activePlayerIndex !== game.playerIndex  || game.currentRoundPhase === RoundPhase.PlayFirstCards} 
      onClick={() => placebid(bidNumber)}>Bid {bidNumber}</Button>
    })
    }
  </ListItem>}
  {!game.gameComplete && game.currentRoundPhase === RoundPhase.Flipping && 
  game.activePlayerIndex === game.playerIndex && 
  <ListItem key="flip-card">
    {game.roundCountPlayerCardsPlayed[roundIndex].map((playedCount, i) => {
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
    })}
  </ListItem>}
</List>
</CardContent>
</Card>
}


export function GameComponent() {
  const { gameId } = useParams<{ gameId: string }>();
  const [connection, setConnection] = useState<HubConnection | undefined>(undefined);
  const [error, setError] = useState(false);
  const [loading, setLoading] = useState(true);
  const [game, setGame] = useState<PlayerGame | PublicGame | undefined>(undefined);

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
    .withUrl(API_ROOT_URL + '/game/ws')
    .build()

    setConnection(newConnection)
  }, [setConnection])

  useEffect(() => {
    if (gameId) {
      updateGame(gameId, setGame, setError, setLoading);
    }
  }, [gameId, setGame, setError, setLoading])

  useEffect(() => {
    if (gameId && connection) {
      connection.on("send", msg => {
        if (msg.activity === "Turn" && msg.id === gameId) {
          updateGame(gameId, setGame, setError, setLoading);
        }
      })
      connection.start()
      .then(() => connection.send("subscribeToGameId", gameId))
      .catch(e => console.log('Connection failed: ', e));
    }
    return () => {
      connection && connection.stop();
    }
  }, [connection, gameId, setGame, setError, setLoading]);

  const idArray = [];
  if (game) {
    for (let i = 0; i < game.playerCount; i++) {
      idArray.push(i)
    }
  }

  return (<>
  {!game && <Card>
    <CardContent>
      {loading && <CircularProgress />}
      {error && <Alert severity="error">Error loading</Alert>}
    </CardContent>
  </Card>}
  {game && idArray.map(i => <PublicPlayerView key={i} index={i} game={game}/>)}
  {game && 'playerId' in game && <GameControlsComponent game={game}/>}
  </>)
}