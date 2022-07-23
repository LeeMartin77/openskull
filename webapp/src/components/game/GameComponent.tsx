import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { Alert, Card, CardContent, CardHeader, CircularProgress, Grid } from "@mui/material";
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { API_ROOT_URL, USER_ID, USER_ID_HEADER, USER_SECRET, USER_SECRET_HEADER } from "../../config";
import { CardType, PlayerGame, PublicGame, RoundPhase } from "../../models/Game";

import FlowerIcon from '@mui/icons-material/LocalFlorist';
import LostCardIcon from '@mui/icons-material/DoNotDisturbOn';
import CircleIcon from '@mui/icons-material/Circle';
import SkullIcon from '@mui/icons-material/Balcony';
import { GameControlsComponent } from "./GameControlsComponent";

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

function PublicPlayerView({ index, game }: { index: number, game: PublicGame | PlayerGame }) {
  const isMe = 'playerIndex' in game && index === game.playerIndex
  const roundIndex = game.roundNumber - 1;
  const wins = game.roundWinners.filter(x => x === index).length;
  const playerRevealedCards = game.roundPlayerCardsRevealed[roundIndex][index].map((card, i) => card === CardType.Flower ? <FlowerIcon key={"revealed-"+i} /> : <SkullIcon key={"revealed-"+i} />);
  const playedRevealedCardsCount = game.roundPlayerCardsRevealed[roundIndex][index].length;
  const unrevealedCardsPlayed = game.roundCountPlayerCardsPlayed[roundIndex][index] - playedRevealedCardsCount;
  const cardsUnplayed = game.currentCountPlayerCardsAvailable[index] - game.roundCountPlayerCardsPlayed[roundIndex][index];
  const cardsLost = game.playerCardStartingCount - game.currentCountPlayerCardsAvailable[index];
  const currentBid = game.currentBids[index] === SKIP_VALUE ? "Withdrawn" : game.currentBids[index] === 0 ? "No Bid" : game.currentBids[index]
  return <Card variant={game.activePlayerIndex === index ? "outlined" : undefined}>
    <CardHeader title={isMe ? '(You) ' + game.playerNicknames[index] : game.playerNicknames[index]} subheader={wins + " point(s)"}></CardHeader>
    <CardContent>
      <div>
        {playerRevealedCards}
        {Array.from(Array(unrevealedCardsPlayed).keys()).map(i => <CircleIcon key={"hidden-"+i}/>)}
        {Array.from(Array(cardsUnplayed).keys()).map(i => <CircleIcon key={"unplayed-"+i} color="disabled" />)}
        {Array.from(Array(cardsLost).keys()).map(i => <LostCardIcon key={"lost-"+i} color="disabled" />)}
      </div>
      {(game.currentRoundPhase === RoundPhase.Bidding || (game.currentRoundPhase === RoundPhase.Flipping && game.activePlayerIndex === index)) && <div>
        Bid: {currentBid}
      </div>}
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
  {game && <Grid container spacing={2} style={{ marginBottom: '1em' }}>
      {idArray.map(i => <Grid item key={i} xs={12} sm={6}><PublicPlayerView index={i} game={game}/></Grid>)}
  </Grid>}
  {game && 'playerId' in game && !game.gameComplete && <GameControlsComponent game={game}/>}
  </>)
}