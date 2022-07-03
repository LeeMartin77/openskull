import { useEffect, useState } from "react";
import "./App.css";
import {
  Route,
  Routes,
  BrowserRouter
} from "react-router-dom";
import { ThemeProvider } from "@emotion/react";
import { Container, createTheme, CssBaseline, Box, Card, CardContent } from "@mui/material";
import { GameQueueComponent } from "./components/queues/GameQueueComponent";
import { GameComponent } from "./components/game/GameComponent";
import { GameListComponent } from "./components/game/GameListComponent";

const theme = createTheme({
  palette: {
    mode: 'dark',
  },
});

function App() {
  const [isDesktop, setDesktop] = useState(window.innerWidth > theme.breakpoints.values.sm);

  const updateMedia = () => {
    setDesktop(window.innerWidth > theme.breakpoints.values.sm);
  };

  useEffect(() => {
    window.addEventListener("resize", updateMedia);
    return () => window.removeEventListener("resize", updateMedia);
  });

  const containerClassName = isDesktop ? "main-container-nonmobile" : "main-container-mobile";

  const mainSx = isDesktop ? {} : {minWidth: "100%"}
  return (
    <ThemeProvider theme={theme}>
        <CssBaseline />
        <BrowserRouter>

          <Box sx={{ display: 'flex' }}>
          <Box component="main" sx={mainSx}>
            <Container maxWidth={'sm'} className={containerClassName} >
            <Routes>
              <Route path="/games/:gameId" element={<GameComponent />} />
              <Route path="/games" element={<GameListComponent />} />
              <Route path="/queue" element={<GameQueueComponent />} />
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
            </Routes>
            </Container>
          </Box>
          </Box>
        </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;