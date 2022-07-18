import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { Alert, Button, Card, CardContent, CircularProgress, List, ListItemButton, ListItemText } from "@mui/material";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { API_ROOT_URL, USER_ID } from "../../config";
import { IQueueStatus } from "../../models/Queue";

export function GameQueueComponent() {
  const navigate = useNavigate();
  const [connection, setConnection] = useState<HubConnection | undefined>(undefined);
  const [error, setError] = useState(false)
  const [loading, setLoading] = useState(true)
  const [queueStatus, setQueueStatus] = useState<IQueueStatus | undefined>(undefined)
  const [gameId, setGameId] = useState<string | undefined>(undefined);

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
    .withUrl(API_ROOT_URL + '/player/ws')
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
      const msgHnld = (msg: any) => {
        if (msg.activity === "GameCreated") {
          setGameId(msg.id);
        }
        if (msg.activity === "QueueLeft") {
          setLoading(false);
          setQueueStatus(undefined);
        }
        if (msg.activity === "QueueStatus" || msg.activity === "QueueJoined") {
          setLoading(false);
          if (msg.details.gameSize === 0) {
            setQueueStatus(undefined)
          } else {
            setQueueStatus(msg.details)
          }
        }
      }
      
      connection.on("send", msg => msgHnld(msg))
  
      connection.start()
      .then(() => connection.send("subscribeToUserId", USER_ID))
      .then(() => connection.send("getQueueStatus", USER_ID))
      .catch(() => { setError(true); setLoading(false); });
    }
    return () => {
      connection && connection.stop();
    }
  }, [connection, setGameId, setLoading, setError, setQueueStatus]);

  const joinQueue = (gameSize: number) => {
    if (connection) {
      setLoading(true);
      connection.send("JoinQueue", USER_ID, gameSize)
    }
  }

  const leaveQueue = () => {
    if (connection) {
      setLoading(true);
      connection.send("LeaveQueues", USER_ID)
    }
  }
  const QUEUE_SIZES = [3, 4, 5, 6];

  return (
    <Card>
      {loading && <CardContent>
        <CircularProgress />
      </CardContent>}
      {!loading && <CardContent>
        {error && <Alert severity="error">Error</Alert>}
        {!queueStatus && !gameId && <List>
          {QUEUE_SIZES.map(size => 
            <ListItemButton key={`${size}-queue-button`} onClick={() => joinQueue(size)}>
              <ListItemText>Join {size} Player Queue</ListItemText>
            </ListItemButton>)}
          </List>}
        {queueStatus && !gameId && <Button onClick={() => leaveQueue()}>Leave Queue</Button>}
        {gameId && <Button onClick={() => navigate("/games/" + gameId)}>Go to Game</Button>}
      </CardContent>}
    </Card>
    )
}