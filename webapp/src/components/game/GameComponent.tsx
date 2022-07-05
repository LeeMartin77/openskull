import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { Alert, Button, Card, CardContent, CircularProgress, List, ListItem } from "@mui/material";
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { API_ROOT_URL, USER_ID, USER_ID_HEADER } from "../../config";
import { CardState, CardType, PlayerGame, PublicGame, TurnAction } from "../../models/Game";


const updateGame = (
  gameId: string, 
  fnSetGame: React.Dispatch<React.SetStateAction<PlayerGame | PublicGame | undefined>>,
  fnSetError: React.Dispatch<React.SetStateAction<boolean>>, 
  fnSetLoading: React.Dispatch<React.SetStateAction<boolean>>
  ) => {
  fnSetLoading(true);
  return fetch(`${API_ROOT_URL}/games/${gameId}`, 
  { headers: { "Content-Type": "application/json", [USER_ID_HEADER]: USER_ID }})
  .then(async res => {
    const item = await res.json();
    fnSetGame(item);
  })
  .catch(() => fnSetError(true))
  .finally(() => fnSetLoading(false))
}

function PublicPlayerView({ index, game }: { index: number, game: PublicGame }) {
  const roundIndex = game.roundNumber - 1;
  return <List>
    <ListItem key="available">Cards Available: {game.currentCountPlayerCardsAvailable[index]}/{game.playerCardStartingCount}</ListItem>
    <ListItem key="bid">Current Bid: {game.currentBids[index]}</ListItem>
    <ListItem key="played">Cards Played: {game.roundCountPlayerCardsPlayed[roundIndex][index]}</ListItem>
  </List>
}

function PrivatePlayerView({ game }: { game: PlayerGame }) {
  const roundIndex = game.roundNumber - 1;
  
  const [clicked, setClicked] = useState(false);

  const playCard = (cardId: string) => {
    setClicked(true);
    fetch(`${API_ROOT_URL}/games/${game.id}/turn`, 
      { 
        method: "POST",
        headers: { "Content-Type": "application/json", [USER_ID_HEADER]: USER_ID },
        body: JSON.stringify({
          action: TurnAction[TurnAction.Card],
          cardId
        })
      })
      .finally(() => setClicked(false))
  }

  return <List>
  <ListItem key="available">Cards Available: {game.currentCountPlayerCardsAvailable[game.playerIndex]}/{game.playerCardStartingCount}</ListItem>
  <ListItem key="bid">Current Bid: {game.currentBids[game.playerIndex]}</ListItem>
  <ListItem key="played">Cards Played: {game.roundCountPlayerCardsPlayed[roundIndex][game.playerIndex]}</ListItem>
  <ListItem key="play-card">
    {game.playerCards.map(card => {
      return <Button
      key={card.id}
      onClick={() => playCard(card.id)}
      color={card.state === CardState.Discarded ? "error" : "primary"}
      disabled={clicked || game.activePlayerIndex !== game.playerIndex || card.state === CardState.Discarded || game.playerRoundCardIdsPlayed[roundIndex].includes(card.id)}
      >
        Play {CardType[card.type]}
      </Button>
    })}
  </ListItem>
</List>
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
  {game && idArray.map(i => {
    return <Card key={i} variant={game.activePlayerIndex === i ? "outlined" : undefined}>
    <CardContent>
      {'playerId' in game && game.playerIndex === i && <PrivatePlayerView game={game}/>}
      {(!('playerId' in game) || game.playerIndex !== i) && <PublicPlayerView index={i} game={game}/>}
      </CardContent>
      </Card>
  })}</>)
}