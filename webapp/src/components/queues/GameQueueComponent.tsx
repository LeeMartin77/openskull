import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { Alert, Button, Card, CardContent, CircularProgress } from "@mui/material";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { API_ROOT_URL, USER_ID, USER_ID_HEADER } from "../../config";
import { IQueueStatus } from "../../models/Queue";

const getQueueStatus = (
  setQueueStatus: React.Dispatch<React.SetStateAction<IQueueStatus | undefined>>, 
  setLoading: React.Dispatch<React.SetStateAction<boolean>>, 
  setError: React.Dispatch<React.SetStateAction<boolean>>, 
  fnFetch = fetch
  ) => {
  setLoading(true);
  return fnFetch(`${API_ROOT_URL}/games/join`, { headers: { "Content-Type": "application/json", [USER_ID_HEADER]: USER_ID }})
  .then(res => {
    if (res.status === 204) {
      setQueueStatus(undefined);
    }
    if (res.status === 200) {
      res.json().then(setQueueStatus);
    }
  })
  .catch(() => setError(true))
  .finally(() => setLoading(false))
}

export function GameQueueComponent() {
  const navigate = useNavigate();
  const [connection, setConnection] = useState<HubConnection | undefined>(undefined);
  const [error, setError] = useState(false)
  const [loading, setLoading] = useState(true)
  const [queueStatus, setQueueStatus] = useState<IQueueStatus | undefined>(undefined)
  const [gameId, setGameId] = useState<string | undefined>(undefined);

  useEffect(() => {
    console.log(USER_ID + " 1")
    const newConnection = new HubConnectionBuilder()
    .withUrl(API_ROOT_URL + '/player/ws')
    .configureLogging(LogLevel.Debug)
    .build()

    setConnection(newConnection)
  }, [setConnection])

  useEffect(() => {
    if (gameId) {
      navigate("/games/" + gameId)
    }
  },[gameId, navigate])

  useEffect(() => {
    if (connection) {
      console.log(USER_ID + " 2")

      const msgHnld = (msg: any, fnSetGameId = setGameId) => {
        if (msg.activity === "GameCreated") {
          fnSetGameId(msg.id);
        }
      }
      
      connection.on("send", msg => msgHnld(msg, setGameId))
  
      connection.start()
      .then(() => connection.send("subscribeToUserId", USER_ID))
      .catch(e => console.log('Connection failed: ', e));

    }
    return () => {
      connection && connection.stop();
    }
  }, [connection, setGameId]);

  useEffect(() => {
    getQueueStatus(setQueueStatus, setLoading, setError)
  }, [setLoading, setError, setQueueStatus])

  const joinQueue = (gameSize: number) => {
    setLoading(true);
    return fetch(`${API_ROOT_URL}/games/join`, { method: "POST",  headers: { "Content-Type": "application/json", [USER_ID_HEADER]: USER_ID }, body: JSON.stringify({ GameSize: gameSize }) })
      .then(res => {
        if (res.status === 204) {
          getQueueStatus(setQueueStatus, setLoading, setError)
        }
        if (res.status === 200) {
          // Game was created!
          res.json().then(game => navigate("/games/" + game.id));
        }
      })
  }

  
  const leaveQueue = () => {
    setLoading(true);
    return fetch(`${API_ROOT_URL}/games/leave`, { method: "POST", headers: { "Content-Type": "application/json", [USER_ID_HEADER]: USER_ID } })
      .then(() => getQueueStatus(setQueueStatus, setLoading, setError))
  }


  return (
    <Card>
      {loading && <CardContent>
        <CircularProgress />
      </CardContent>}
      {!loading && <CardContent>
        {error && <Alert severity="error">Error</Alert>}
        {!queueStatus && !gameId && <Button onClick={() => joinQueue(3)}>Join 3 Player Queue</Button>}
        {queueStatus && !gameId && <Button onClick={() => leaveQueue()}>Leave Queue</Button>}
        {gameId && <Button onClick={() => navigate("/games/" + gameId)}>Go to Game</Button>}
      </CardContent>}
    </Card>
    )
}