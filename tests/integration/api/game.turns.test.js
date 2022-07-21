const signalR = require('@microsoft/signalr');
const https = require('https');
const axios = require("axios").create({
  httpsAgent: new https.Agent({  
    rejectUnauthorized: false
  })
});
const crypto = require("crypto");

const apiRoot = process.env.OPENSKULL_API_ROOT ?? "http://localhost:5248"

test("Game Turns Played :: Get websocket turn notification", async () => {
  const TEST_PLAYER_IDS = [
    [crypto.randomUUID(), crypto.randomUUID()],
    [crypto.randomUUID(), crypto.randomUUID()],
    [crypto.randomUUID(), crypto.randomUUID()]
  ] 
  const gameCreateResponse = await axios.post(apiRoot + "/games/createtestgame", {
    playerIds: TEST_PLAYER_IDS.map(x => x[0])
  });
  expect(gameCreateResponse.status).toBe(200);
  const gameId = gameCreateResponse.data;

  const TEST_PLAYER_CARDS = [];
  for (let id of TEST_PLAYER_IDS) {
    const privateGameData = await axios.get(`${apiRoot}/games/${gameId}`,
    {
      headers: {
        "X-OpenSkull-UserId": id[0],
        "X-OpenSkull-UserSecret": id[1]
      }
    })
    TEST_PLAYER_CARDS.push(privateGameData.data.playerCards)
  }

  const gameMessages = [
    [],
    [],
    []
  ]

  const gameResponseHandler = (index, msg, msgs = gameMessages) => {
    msgs[index].push(msg)
  }
  
  const gameConnections = await Promise.all(TEST_PLAYER_IDS.map(async (id, i) => {
    let connection = new signalR.HubConnectionBuilder()
    .withUrl(apiRoot +`/game/ws`)
    .configureLogging(signalR.LogLevel.Error)
    .build();

    connection.on("send", msg => gameResponseHandler(i, msg));

    await connection.start();
    await connection.send("subscribeToGameId", gameId)
    return connection;
  }));


  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
    { action: "Card", cardId: TEST_PLAYER_CARDS[0][0].id }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0], "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1] }
  });

  // gotta give it a beat here

  while (gameMessages.reduce((prev, curr) => [...prev, ...curr], []).length < 3) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  expect(gameMessages.every(x => x.every(y => y.id == gameId && y.activity == "Turn") && x.length == 1)).toBe(true);

  await Promise.all(gameConnections.map(async c => await c.stop()))
});