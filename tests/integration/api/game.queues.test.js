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

test("Join Game :: Can connect to queues and get game created", async () => {
  const TEST_PLAYER_IDS = [
    [crypto.randomUUID(), crypto.randomUUID()],
    [crypto.randomUUID(), crypto.randomUUID()],
    [crypto.randomUUID(), crypto.randomUUID()]
  ] 

  const messages = [
    [],
    [],
    []
  ]

  const responseHandler = (index, msg, msgs = messages) => {
    msgs[index].push(msg)
  }

  const connections = [];

  TEST_PLAYER_IDS.forEach(async (id, i) => {
    let connection = new signalR.HubConnectionBuilder()
    .withUrl(apiRoot +`/player/ws`)
    .configureLogging(signalR.LogLevel.Error)
    .build();

    connection.on("send", msg => responseHandler(i, msg));

    await connection.start();

    await connection.send("subscribeToUserId", id[0], id[1])

    while(!messages[i].map(x => x.activity).includes("Subscribed")) {
      await new Promise(resolve => setTimeout(resolve, 10))
    }

    await connection.send("joinQueue", id[0], id[1], 3)

    connections.push(connection);
  });

  while (messages.reduce((prev, curr) => [...prev, ...curr.filter(x => x.activity == "GameCreated")], []).length < 3) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  var createdMessageIndex = messages[0].map(x => x.activity).findIndex(x => x == "GameCreated")
  expect(createdMessageIndex).not.toBe(-1);

  const gameId = messages[0][createdMessageIndex].id;

  expect(messages.every(x => x.filter(y => y.id == gameId && y.activity == "GameCreated").length == 1)).toBe(true);
  await Promise.all(connections.map(async c => await c.stop()))
});

test("Can query, queue, query, leave, query", async () => {
  const playerId = v4();
  const secret = v4();

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

  await connection.send("subscribeToUserId", playerId, secret)

  while (messages.length < 1) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  expect(messages[0].activity).toBe("Subscribed")

  messages.pop()

  await connection.send("getQueueStatus", playerId, secret)

  while (messages.length < 1) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  expect(messages[0].activity).toBe("QueueStatus")
  expect(messages[0].details.gameSize).toBe(0)
  expect(messages[0].details.queueSize).toBe(0)

  await connection.send("joinQueue", playerId, secret, 4)

  while (messages.length < 2) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  expect(messages[1].activity).toBe("QueueJoined")
  expect(messages[1].details.gameSize).toBe(4)
  expect(messages[1].details.queueSize).toBe(1)

  await connection.send("getQueueStatus", playerId, secret)

  while (messages.length < 3) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  expect(messages[2].activity).toBe("QueueStatus")
  expect(messages[2].details.gameSize).toBe(4)
  expect(messages[2].details.queueSize).toBe(1)

  await connection.send("leaveQueues", playerId, secret)

  while (messages.length < 4) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  expect(messages[3].activity).toBe("QueueLeft")

  await connection.send("getQueueStatus", playerId, secret)

  while (messages.length < 5) {
    await new Promise(resolve => setTimeout(resolve, 10))
  }

  expect(messages[4].activity).toBe("QueueStatus")
  expect(messages[4].details.gameSize).toBe(0)
  expect(messages[4].details.queueSize).toBe(0)

  await connection.stop();
})