import { HubConnection } from "@microsoft/signalr";
import { Alert, Button, Card, CardContent, CircularProgress, List, ListItemButton, ListItemText } from "@mui/material";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { USER_ID, USER_SECRET } from "../../config";
import { IOpenskullMessage } from "../../models/Message";
import { IQueueStatus } from "../../models/Queue";

export function GameQueueComponent({ connection }: { connection: HubConnection }) {
  const navigate = useNavigate();
  const [error, setError] = useState(false)
  const [loading, setLoading] = useState(true)
  const [queueStatus, setQueueStatus] = useState<IQueueStatus | undefined>(undefined)
  const [gameId, setGameId] = useState<string | undefined>(undefined);

  useEffect(() => {
    if (gameId) {
      navigate("/games/" + gameId)
    }
  },[gameId, navigate])

  useEffect(() => {

    const msgHnld = (msg: IOpenskullMessage) => {
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
    if (connection) {
      
      connection.on("send", msg => msgHnld(msg))

      connection.send("getQueueStatus", USER_ID, USER_SECRET)
    }
    return () => {
      connection && connection.off("send");
    }
  }, [connection, setGameId, setLoading, setError, setQueueStatus]);

  const joinQueue = (gameSize: number) => {
    if (connection) {
      setLoading(true);
      connection.send("JoinQueue", USER_ID, USER_SECRET, gameSize)
    }
  }

  const leaveQueue = () => {
    if (connection) {
      setLoading(true);
      connection.send("LeaveQueues", USER_ID, USER_SECRET)
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