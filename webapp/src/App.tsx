import { useEffect, useState } from "react";
import "./App.css";
import {
  Route,
  Routes,
  BrowserRouter
} from "react-router-dom";
import { ThemeProvider } from "@emotion/react";
import { Container, createTheme, CssBaseline, Box, Card, CardContent, Alert, CircularProgress } from "@mui/material";
import { GameQueueComponent } from "./components/queues/GameQueueComponent";
import { GameComponent } from "./components/game/GameComponent";
import { GameListComponent } from "./components/game/GameListComponent";
import { Link } from "react-router-dom";
import { List, ListItem, ListItemButton, ListItemIcon, ListItemText } from '@mui/material';
import { Queue, ViewList } from '@mui/icons-material';
import { BottomNavigationComponent } from "./components/navigation/BottomNavigationComponent";
import { SideNavigationComponent } from "./components/navigation/SideNavigationComponent";
import { API_ROOT_URL, USER_ID, USER_NICKNAME, USER_SECRET } from "./config";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { IOpenskullMessage } from "./models/Message";
import { GameCreatedModalComponent } from "./components/game/GameCreatedComponent";
import { UpdateUserProfileButton } from "./components/player/UpdatePlayerNicknameDialog";
import { PlayerRoomComponent } from "./components/rooms/PlayerRoomComponent";
import { PlayerRoomDialogComponent } from "./components/rooms/PlayerRoomDialogComponent";

const theme = createTheme({
  palette: {
    mode: 'dark',
  },
});

function HomeComponent() {
  return <Card>
      <CardContent>
        <List>
          <ListItem>
          <ListItemButton component={Link} to={"/queue"}>
            <ListItemIcon><Queue/></ListItemIcon>
            <ListItemText>Game Queue</ListItemText>
          </ListItemButton>
          <ListItemButton component={Link} to={"/games"}>
            <ListItemIcon><ViewList/></ListItemIcon>
            <ListItemText>Game List</ListItemText>
          </ListItemButton>
        </ListItem>
      </List>
    </CardContent>
  </Card>
}

function App() {
  const [isDesktop, setDesktop] = useState(window.innerWidth > theme.breakpoints.values.sm);
  const [playerConnection, setPlayerConnection] = useState<HubConnection | undefined>(undefined);
  const [userNickname, setUserNickname] = useState(USER_NICKNAME);
  const [newGameId, setNewGameId] = useState<string | undefined>(undefined);
  const [error, setError] = useState(false)
  const [loading, setLoading] = useState(true)
  const [roomDialogOpen, setRoomDialogOpen] = useState(false);

  const updateMedia = () => {
    setDesktop(window.innerWidth > theme.breakpoints.values.sm);
  };

  useEffect(() => {
    window.addEventListener("resize", updateMedia);
    return () => window.removeEventListener("resize", updateMedia);
  });


  useEffect(() => {
    const newPlayerConnection = new HubConnectionBuilder()
    .withUrl(API_ROOT_URL + '/player/ws')
    .build()

    const playerSuccessHandler = (msg: IOpenskullMessage) => {
      if (msg.activity === "Subscribed" && msg.id === USER_ID) {
        setPlayerConnection(newPlayerConnection)
        setError(false)
        setLoading(false)
      }
    }

    const newGameHandler = (msg: IOpenskullMessage) => {
      if (msg.activity === "GameCreated") {
        setNewGameId(msg.id);
      }
    }

    newPlayerConnection.on("send", newGameHandler)

    newPlayerConnection.on("send", playerSuccessHandler)

    newPlayerConnection.start()
      .then(() => newPlayerConnection.send("subscribeToUserId", USER_ID, USER_SECRET))
      .catch(() => setError(true))

    return () => {
      newPlayerConnection && newPlayerConnection.stop();
    }
  }, [setPlayerConnection, setNewGameId])

  const handleDismissNewGameDialogue = () => setNewGameId(undefined);

  const containerClassName = isDesktop ? "main-container-nonmobile" : "main-container-mobile";

  const mainSx = isDesktop ? {} : {minWidth: "100%"}
  return (
    <ThemeProvider theme={theme}>
        <CssBaseline />
        {playerConnection && <UpdateUserProfileButton
          playerConnection={playerConnection}
          playerNickname={userNickname}
          updatePlayerNickname={setUserNickname}
        />}
        <BrowserRouter>
          {playerConnection && <PlayerRoomDialogComponent open={roomDialogOpen} setOpen={setRoomDialogOpen}/>}
          <Box sx={{ display: 'flex' }}>
          {!loading && isDesktop && <SideNavigationComponent setRoomDialogOpen={setRoomDialogOpen}/>}
          <Box component="main" sx={mainSx}>
            <Container className={containerClassName} >
            {loading && <CircularProgress />}
            <GameCreatedModalComponent 
              gameId={newGameId} 
              handleDismiss={handleDismissNewGameDialogue}
              />
            {!loading && error && <Alert color="error" >Error Connecting to Server</Alert>}
            {!loading && <Routes>
              <Route path="/games/:gameId" element={<GameComponent />} />
              <Route path="/games" element={<GameListComponent />} />
              <Route path="/rooms/:roomId" element={playerConnection && <PlayerRoomComponent playerConnection={playerConnection} />} />
              <Route path="/queue" element={playerConnection && <GameQueueComponent connection={playerConnection} />} />
              <Route path="/" element={<HomeComponent />}/>
              <Route
                path="*"
                element={
                  <Card>
                    <CardContent>
                      <p>There's nothing here!</p>
                    </CardContent>
                  </Card>
                }
              />
            </Routes>}
            </Container>
          </Box>
          {!loading && !isDesktop && <BottomNavigationComponent setRoomDialogOpen={setRoomDialogOpen}/>}
          </Box>
        </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;