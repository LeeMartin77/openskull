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

jest.setTimeout(20000);

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

  for (var i = 0; i++; i < TEST_PLAYER_IDS.length) {
    await new Promise(resolved => setTimeout(resolved, 100));
    await connections[i].send("joinQueue", TEST_PLAYER_IDS[i], 3)
  }

  var waiting = true;
  while (waiting) {
    await new Promise(resolve => setTimeout(resolve, 100))
    waiting = !messages.every(y => y.map(x => x.activity).includes("GameCreated"));
  }
  
  var createdMessageIndex = messages[0].map(x => x.activity).findIndex(x => x == "GameCreated")
  expect(createdMessageIndex).not.toBe(-1);

  const gameId = messages[0][createdMessageIndex].id;

  expect(messages.every(x => x.filter(y => y.id == gameId && y.activity == "GameCreated").length == 1)).toBe(true);
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

// TODO: Test Status/Queue/Leave/Status loop

test("Can query, queue, query, leave, query", async () => {
  const playerId = v4();

  const messages = [];

  const messageHandler = (msg, msgs = messages) => {
    msgs.push(msg);
  }

  let connection = new signalR.HubConnectionBuilder()
  .withUrl(apiRoot +`/player/ws`)
  .configureLogging(signalR.LogLevel.Error)
  .build();

  connection.on("send", messageHandler);

  await connection.start();

  await connection.send("subscribeToUserId", playerId)

  await connection.send("getQueueStatus", playerId)

  while (messages.length < 1) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  expect(messages[0].activity).toBe("QueueStatus")
  expect(messages[0].details.gameSize).toBe(0)
  expect(messages[0].details.queueSize).toBe(0)

  await connection.send("joinQueue", playerId, 4)

  while (messages.length < 2) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  expect(messages[1].activity).toBe("QueueJoined")
  expect(messages[1].details.gameSize).toBe(4)
  expect(messages[1].details.queueSize).toBe(1)

  await connection.send("getQueueStatus", playerId)

  while (messages.length < 3) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  expect(messages[2].activity).toBe("QueueStatus")
  expect(messages[2].details.gameSize).toBe(4)
  expect(messages[2].details.queueSize).toBe(1)

  await connection.send("leaveQueues", playerId)

  while (messages.length < 4) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  expect(messages[3].activity).toBe("QueueLeft")
  //expect(messages[3].details.gameSize).toBe(4)

  await connection.send("getQueueStatus", playerId)

  while (messages.length < 5) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  expect(messages[4].activity).toBe("QueueStatus")
  expect(messages[4].details.gameSize).toBe(0)
  expect(messages[4].details.queueSize).toBe(0)

  await connection.stop();
})