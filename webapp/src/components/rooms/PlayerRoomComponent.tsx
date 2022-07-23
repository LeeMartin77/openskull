import { HubConnection } from "@microsoft/signalr";
import { Alert, Button, Card, CardContent, Link, List, ListItem, ListItemText } from "@mui/material";
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { USER_ID, USER_SECRET } from "../../config";
import { IOpenskullMessage } from "../../models/Message";
import { Link as LinkIcon } from "@mui/icons-material"

interface IPlayerRoomComponentProps {
  playerConnection: HubConnection
}

export function PlayerRoomComponent({ playerConnection }: IPlayerRoomComponentProps) {

  const { roomId } = useParams<{ roomId: string }>();
  const [playersInRoom, setPlayersInRoom] = useState<string[]>([])

  useEffect(() => {

    const msgHnld = (msg: IOpenskullMessage) => {
      if (msg.activity === "RoomUpdate" && msg.roomDetails.roomId === roomId) {
        setPlayersInRoom(msg.roomDetails.playerNicknames);
      }
    }
    playerConnection.on("send", msgHnld)

    playerConnection.send("joinRoom", USER_ID, USER_SECRET, roomId)
    return () => {
      playerConnection.send("leaveRoom", USER_ID, USER_SECRET, roomId)
      playerConnection.off("send", msgHnld);
    }
  }, [playerConnection, roomId, setPlayersInRoom]);

  const createGameFromRoom = () => {
    playerConnection.send("createRoomGame", USER_ID, USER_SECRET, roomId)
  }

  const validNumberOfPlayersInRoom = playersInRoom.length > 2 && playersInRoom.length < 7;

  return <Card>
  <CardContent>
    {playerConnection.state === "Disconnected" && <Alert severity="error">Connection Disconnected</Alert>}
    {playersInRoom.length === 0 && <Alert severity="warning">No players in room?</Alert>}
    <Button 
      onClick={createGameFromRoom}
      disabled={!validNumberOfPlayersInRoom}
      variant="contained">Create Game</Button>
    <Button 
      component={Link} 
      href={"/rooms/" + roomId} 
      color="secondary" 
      variant="outlined"
      endIcon={<LinkIcon />}
      >Link</Button>
    <List>
      {playersInRoom.map((player, i) => 
        <ListItem key={i}>
          <ListItemText>{player}</ListItemText>
        </ListItem>)}
    </List>
  </CardContent>
</Card>
}