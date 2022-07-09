import { Alert, Card, CardContent, CircularProgress, List, ListItemButton, ListItemText } from "@mui/material";
import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { API_ROOT_URL, USER_ID, USER_ID_HEADER } from "../../config";
import { PlayerGame } from "../../models/Game";

const getGames = (
  fnSetGames: React.Dispatch<React.SetStateAction<PlayerGame[] | undefined>>,
  fnSetError: React.Dispatch<React.SetStateAction<boolean>>, 
  fnSetLoading: React.Dispatch<React.SetStateAction<boolean>>
  ) => {
  fnSetLoading(true);
  return fetch(`${API_ROOT_URL}/games`, 
  { headers: { "Content-Type": "application/json", [USER_ID_HEADER]: USER_ID }})
  .then(async res => {
    const item = await res.json();
    fnSetGames(item);
  })
  .catch(() => fnSetError(true))
  .finally(() => fnSetLoading(false))
}


export function GameListComponent() {
  const [error, setError] = useState(false);
  const [loading, setLoading] = useState(true);
  const [games, setGames] = useState<PlayerGame[] | undefined>(undefined);

  useEffect(() => {
    getGames(setGames, setError, setLoading);
  }, [setGames, setError, setLoading])

  return     <Card>
  {loading && <CardContent>
    <CircularProgress />
  </CardContent>}
  {!loading && <CardContent>
    {error && <Alert severity="error">Error</Alert>}
    {games && games.length > 0 && <List>
      {games.map(game => 
        <ListItemButton key={game.id} component={Link} to={`/games/${game.id}`}>
          <ListItemText>{game.playerCount} Players: {game.gameComplete ? "Complete": "Ongoing"}</ListItemText>
        </ListItemButton>)}
    </List>}
    {(!games || games.length === 0) && <Alert severity="warning">No games to show</Alert>}
  </CardContent>}
  </Card>
}