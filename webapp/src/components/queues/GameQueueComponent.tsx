import { HubConnection } from "@microsoft/signalr";
import { Alert, Button, Card, CardContent, CircularProgress, List, ListItemButton, ListItemText } from "@mui/material";
import { useEffect, useState } from "react";
import { USER_ID, USER_SECRET } from "../../config";
import { IOpenskullMessage } from "../../models/Message";
import { IQueueStatus } from "../../models/Queue";

export function GameQueueComponent({ connection }: { connection: HubConnection }) {
  const [error, setError] = useState(false)
  const [loading, setLoading] = useState(true)
  const [queueStatus, setQueueStatus] = useState<IQueueStatus | undefined>(undefined)

  useEffect(() => {

    const msgHnld = (msg: IOpenskullMessage) => {
      if (msg.activity === "GameCreated") {
        connection.send("getQueueStatus", USER_ID, USER_SECRET)
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
      connection.on("send", msgHnld)

      connection.send("getQueueStatus", USER_ID, USER_SECRET)
    }
    return () => {
      if (connection) {
        connection.send("LeaveQueues", USER_ID, USER_SECRET)
        connection.off("send", msgHnld);
      } 
    }
  }, [connection, setLoading, setError, setQueueStatus]);

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
        {!queueStatus && <List>
          {QUEUE_SIZES.map(size => 
            <ListItemButton key={`${size}-queue-button`} onClick={() => joinQueue(size)}>
              <ListItemText>Join {size} Player Queue</ListItemText>
            </ListItemButton>)}
          </List>}
        {queueStatus && <Button onClick={() => leaveQueue()}>Leave Queue</Button>}
      </CardContent>}
    </Card>
    )
}