import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { Alert, Button, Card, CardContent, CardHeader, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Grid } from "@mui/material";
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { API_ROOT_URL, USER_ID, USER_ID_HEADER, USER_SECRET, USER_SECRET_HEADER } from "../../config";
import { CardType, PlayerGame, PublicGame, RoundPhase } from "../../models/Game";

import FlowerIcon from '@mui/icons-material/LocalFlorist';
import LostCardIcon from '@mui/icons-material/DoNotDisturbOn';
import CircleIcon from '@mui/icons-material/Circle';
import SkullIcon from '@mui/icons-material/Balcony';
import { GameControlsComponent } from "./controls/GameControlsComponent";

const SKIP_VALUE = -1;

const updateGame = (
  gameId: string,
  fnSetGame: React.Dispatch<React.SetStateAction<PlayerGame | PublicGame | undefined>>,
  fnSetError: React.Dispatch<React.SetStateAction<string | undefined>>, 
  fnSetLoading: React.Dispatch<React.SetStateAction<boolean>>
  ) => {
  fnSetLoading(true);
  return fetch(`${API_ROOT_URL}/games/${gameId}`, 
  { headers: { "Content-Type": "application/json", [USER_ID_HEADER]: USER_ID, [USER_SECRET_HEADER]: USER_SECRET }})
  .then(async res => {
    if (res.status === 200) {
      const item = await res.json();
      fnSetError(undefined)
      fnSetGame(item);
    }
    if (res.status === 404) {
      fnSetError("Game not found")
    }
  })
  .catch(() => fnSetError("Error fetching data"))
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
      <div style={{ display: 'flex', alignItems: 'center' }}>
        {playerRevealedCards}
        {Array.from(Array(unrevealedCardsPlayed).keys()).map(i => <CircleIcon style={{width: "1.5em", height: "1.5em"}} key={"hidden-"+i}/>)}
        {Array.from(Array(cardsUnplayed).keys()).map(i => <CircleIcon style={{width: "1.5em", height: "1.5em"}} key={"unplayed-"+i} color="disabled" />)}
        {Array.from(Array(cardsLost).keys()).map(i => <LostCardIcon style={{width: "1.5em", height: "1.5em"}} key={"lost-"+i} color="disabled" />)}
        {(game.currentRoundPhase === RoundPhase.Bidding || (game.currentRoundPhase === RoundPhase.Flipping && game.activePlayerIndex === index)) && <span style={{ marginLeft: '1em' }}>Bid: {currentBid}</span>}
      </div>
    </CardContent>
  </Card>
}

interface IRoundCompleteProps { prevGame: PublicGame | PlayerGame, game: PublicGame | PlayerGame, open: boolean }

function RoundCompleteDialogComponent(
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

interface IGameCompleteDialog { game: PublicGame | PlayerGame, open: boolean }

function GameCompleteDialogComponent(
  { game, open, setGameCompleteDialog }: IGameCompleteDialog & { setGameCompleteDialog : (i: IGameCompleteDialog) => void}
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

function GameUiComponent({ gameId }: { gameId: string }) {
  const [connection, setConnection] = useState<HubConnection | undefined>(undefined);
  const [error, setError] = useState<string | undefined>(undefined);
  const [loading, setLoading] = useState(true);
  const [game, setGame] = useState<PlayerGame | PublicGame | undefined>(undefined);
  const setPrevGame = useState<PlayerGame | PublicGame | undefined>(undefined)[1];
  const [roundChangeDialog, setRoundChangeDialog] = useState<IRoundCompleteProps>()
  const [gameCompleteDialog, setGameCompleteDialog] = useState<IGameCompleteDialog>()

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
    .withUrl(API_ROOT_URL + '/game/ws')
    .build()

    setConnection(newConnection)
  }, [setConnection])

  useEffect(() => {
    setPrevGame(prev => {
      if (game && prev && !game.gameComplete && prev.roundNumber !== game.roundNumber) {
        setRoundChangeDialog({ 
          prevGame: prev,
          game: game,
          open: true
        })
      }
      if (game && game.gameComplete) {
        setGameCompleteDialog({
          open: true,
          game
        })
      }
      return game;
    })
  }, [game, setPrevGame, setRoundChangeDialog])

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
      {error && <Alert severity="error">{error}</Alert>}
    </CardContent>
  </Card>}
  {roundChangeDialog && <RoundCompleteDialogComponent {...roundChangeDialog} setRoundChangeDialog={setRoundChangeDialog} />}
  {gameCompleteDialog && <GameCompleteDialogComponent {...gameCompleteDialog} setGameCompleteDialog={setGameCompleteDialog} />}
  {game && game.gameComplete && <Card>
    <CardContent>
      <Alert severity="info">{game.playerNicknames[game.activePlayerIndex]} wins!</Alert>
    </CardContent>
  </Card>}
  {game && <Grid container spacing={2} style={{ marginBottom: '1em' }}>
      {idArray.map(i => <Grid item key={i} xs={12} sm={6}><PublicPlayerView index={i} game={game}/></Grid>)}
  </Grid>}
  {game && 'playerId' in game && !game.gameComplete && <GameControlsComponent game={game}/>}
  </>)
}

export function GameComponent() {
  const { gameId } = useParams<{ gameId: string }>();
  return gameId ? <GameUiComponent gameId={gameId} /> : <Card>
      <CardContent>
        <Alert severity="error">Missing Game Id</Alert>
      </CardContent>
    </Card>
}