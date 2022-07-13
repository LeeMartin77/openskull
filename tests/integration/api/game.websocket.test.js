const signalR = require('@microsoft/signalr');
const https = require('https');
const axios = require("axios").create({
  httpsAgent: new https.Agent({  
    rejectUnauthorized: false
  })
});
const crypto = require("crypto");
const { v4 } = require("uuid");

const apiRoot = process.env.OPENSKULL_API_ROOT ?? "http://localhost:5248"

jest.setTimeout(10000);

test("Can connect to websockets and get messages", async () => {
  const TEST_PLAYER_IDS = [
    v4(),
    v4(),
    v4()
  ]

  const messages = [
    [],
    [],
    []
  ]

  const responseHandler = (index, msg, msgs = messages) => {
    msgs[index].push(msg)
  }

  const connections = await Promise.all(TEST_PLAYER_IDS.map(async (id, i) => {
    let connection = new signalR.HubConnectionBuilder()
    .withUrl(apiRoot +`/player/ws`)
    .configureLogging(signalR.LogLevel.Error)
    .build();

    connection.on("send", msg => responseHandler(i, msg));

    await connection.start();

    await connection.send("subscribeToUserId", id)
    return connection;
  }));

  TEST_PLAYER_IDS.forEach(async (id, i) => {
    await connections[i].send("joinQueue", id, 3)
  })

  var waiting = true;
  while (waiting) {
    await new Promise(resolve => setTimeout(resolve, 100))
    waiting = !messages[0].map(x => x.activity).includes("GameCreated");
  }
  
  var createdMessageIndex = messages[0].map(x => x.activity).findIndex(x => x == "GameCreated")
  expect(createdMessageIndex).not.toBe(-1);

  const gameId = messages[0][createdMessageIndex].id;

  expect(messages.every(x => x.every(y => y.id == gameId && y.activity == "GameCreated") && x.length == 1)).toBe(true);
  await Promise.all(connections.map(async c => await c.stop()))
  const TEST_PLAYER_CARDS = [];
  for (let id of TEST_PLAYER_IDS) {
    const privateGameData = await axios.get(`${apiRoot}/games/${gameId}`,
    {
      headers: {
        "X-OpenSkull-UserId": id
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
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[0] }
  });

  // gotta give it a beat here
  await new Promise(resolve => setTimeout(resolve, 500))
  
  expect(gameMessages.every(x => x.every(y => y.id == gameId && y.activity == "Turn") && x.length == 1)).toBe(true);

  await Promise.all(gameConnections.map(async c => await c.stop()))
});