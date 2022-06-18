import { useEffect, useState } from "react";
import "./App.css";
import {
  Route,
  Routes,
  BrowserRouter
} from "react-router-dom";
import { ThemeProvider } from "@emotion/react";
import { Container, createTheme, CssBaseline, Box, Card, CardContent, Alert } from "@mui/material";

const theme = createTheme({
  palette: {
    mode: 'dark',
  },
});


interface WeatherForecast
{
    dateTime: Date,
    temperatureC: number,
    temperatureF: number,
    summary?: string
}

const API_ROOT_URL = process.env.REACT_APP_API_ROOT_URL;

export async function getFakeWeather(): Promise<WeatherForecast[]> {
  const response = await fetch(API_ROOT_URL + '/WeatherForecast');
  if (!response.ok) {
    throw Error("Something went wrong")
  }
  return await response.json();
}

function WeatherComponent() {
  const [weather, setWeather] = useState<WeatherForecast[] | undefined>(undefined)
  const [error, setError] = useState(false)

  useEffect(() => {
    getFakeWeather().then(setWeather).catch(() => setError(true))
  }, [setWeather, setError])

  return (
    <Card>
      <CardContent>
        {error && <Alert severity="error">Error</Alert>}
        {weather && <ul>{weather.map((x, i) => <li key={i}>{x.temperatureC}</li>)}</ul>}
      </CardContent>
    </Card>
    )
}

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
              <Route path="/" element={<WeatherComponent />} />
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