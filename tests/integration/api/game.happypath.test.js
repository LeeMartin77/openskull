const signalR = require('@microsoft/signalr');
const https = require('https');
const axios = require("axios").create({
  httpsAgent: new https.Agent({
    rejectUnauthorized: false
  })
});
const crypto = require("crypto");
process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = 0;

const apiRoot = process.env.OPENSKULL_API_ROOT ?? "http://localhost:5248" + "/api"

jest.setTimeout(10000);

test("Happy Path :: Create Test Game Then Play It Through", async () => {
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
  // Card, Bid, Flip
  // cardId, bid, targetPlayerIndex
  // Round One
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Card", cardId: TEST_PLAYER_CARDS[0][0].id },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Card", cardId: TEST_PLAYER_CARDS[1][0].id },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[1][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[1][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Card", cardId: TEST_PLAYER_CARDS[2][0].id },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[2][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[2][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Bid", bid: 1 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Bid", bid: -1 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[1][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[1][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Bid", bid: -1 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[2][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[2][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Flip", targetPlayerIndex: 0 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })

  // Round Two
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Card", cardId: TEST_PLAYER_CARDS[0][0].id },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Card", cardId: TEST_PLAYER_CARDS[1][0].id },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[1][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[1][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Card", cardId: TEST_PLAYER_CARDS[2][0].id },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[2][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[2][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Bid", bid: 1 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Bid", bid: -1 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[1][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[1][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Bid", bid: -1 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[2][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[2][1]
      }
    })
  const final = await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Flip", targetPlayerIndex: 0 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })
  expect(final.data.gameComplete).toBe(true);
  expect(final.data.roundWinners.length).toBe(2);
  expect(final.data.roundWinners[0]).toBe(0);
  expect(final.data.roundWinners[0]).toBe(0);
})

test("Happy Path :: Create Game using Queue Then Play It Through", async () => {
  const TEST_PLAYER_IDS = [
    [crypto.randomUUID(), crypto.randomUUID()],
    [crypto.randomUUID(), crypto.randomUUID()],
    [crypto.randomUUID(), crypto.randomUUID()]
  ]
  var gameId;
  let i = 0;
  const connections = [];
  for (const [id, secret] of TEST_PLAYER_IDS) {
    const messages = [];
    const responseHandler = (msg, msgs = messages) => {
      msgs.push(msg)
    }

    let connection = new signalR.HubConnectionBuilder()
      .withUrl(apiRoot + `/player/ws`)
      .configureLogging(signalR.LogLevel.Error)
      .build();

    connection.on("send", msg => responseHandler(msg));

    await connection.start();

    await connection.send("subscribeToUserId", id, secret)

    while (!messages.map(x => x.activity).includes("Subscribed")) {
      await new Promise(resolve => setTimeout(resolve, 10))
    }

    await connection.send("joinQueue", id, secret, 3)

    while (!messages.map(x => x.activity).includes("GameCreated") && !messages.map(x => x.activity).includes("QueueJoined")) {
      await new Promise(resolve => setTimeout(resolve, 10))
    }

    var createdMessageIndex = messages.map(x => x.activity).findIndex(x => x == "GameCreated")
    if (createdMessageIndex != -1) {
      gameId = messages[createdMessageIndex].id;
    }
  }

  connections.forEach(con => con.stop());

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
  // Card, Bid, Flip
  // cardId, bid, targetPlayerIndex
  // Round One
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Card", cardId: TEST_PLAYER_CARDS[0][0].id },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Card", cardId: TEST_PLAYER_CARDS[1][0].id },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[1][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[1][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Card", cardId: TEST_PLAYER_CARDS[2][0].id },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[2][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[2][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Bid", bid: 1 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Bid", bid: -1 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[1][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[1][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Bid", bid: -1 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[2][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[2][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Flip", targetPlayerIndex: 0 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })

  // Round Two
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Card", cardId: TEST_PLAYER_CARDS[0][0].id },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Card", cardId: TEST_PLAYER_CARDS[1][0].id },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[1][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[1][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Card", cardId: TEST_PLAYER_CARDS[2][0].id },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[2][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[2][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Bid", bid: 1 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Bid", bid: -1 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[1][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[1][1]
      }
    })
  await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Bid", bid: -1 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[2][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[2][1]
      }
    })
  const final = await axios.post(`${apiRoot}/games/${gameId}/turn`,
    { action: "Flip", targetPlayerIndex: 0 },
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })
  expect(final.data.gameComplete).toBe(true);
  expect(final.data.roundWinners.length).toBe(2);
  expect(final.data.roundWinners[0]).toBe(0);
  expect(final.data.roundWinners[0]).toBe(0);
})
