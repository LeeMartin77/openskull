const https = require('https');
const axios = require("axios").create({
  httpsAgent: new https.Agent({  
    rejectUnauthorized: false
  })
});
const crypto = require("crypto");

const apiRoot = process.env.OPENSKULL_API_ROOT ?? "http://localhost:5248"

test("Happy Path :: Create Test Game Then Play It Through", async () => {
  const TEST_PLAYER_IDS = [
    crypto.randomUUID(),
    crypto.randomUUID(),
    crypto.randomUUID()
  ] 
  const gameCreateResponse = await axios.post(apiRoot + "/games/createtestgame", {
    playerIds: TEST_PLAYER_IDS
  });
  expect(gameCreateResponse.status).toBe(200);
  const gameId = gameCreateResponse.data;
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
  // Card, Bid, Flip
  // cardId, bid, targetPlayerIndex
  // Round One
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
    { action: "Card", cardId: TEST_PLAYER_CARDS[0][0].id }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[0] }
  })
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
    { action: "Card", cardId: TEST_PLAYER_CARDS[1][0].id }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[1] }
  })
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
    { action: "Card", cardId: TEST_PLAYER_CARDS[2][0].id }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[2] }
  })
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
    { action: "Bid", bid: 1 }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[0] }
  })
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
  { action: "Bid", bid: -1 }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[1] }
  })
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
  { action: "Bid", bid: -1 }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[2] }
  })
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
    { action: "Flip", targetPlayerIndex: 0 }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[0] }
  })

  // Round Two
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
    { action: "Card", cardId: TEST_PLAYER_CARDS[0][0].id }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[0] }
  })
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
    { action: "Card", cardId: TEST_PLAYER_CARDS[1][0].id }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[1] }
  })
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
    { action: "Card", cardId: TEST_PLAYER_CARDS[2][0].id }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[2] }
  })
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
    { action: "Bid", bid: 1 }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[0] }
  })
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
  { action: "Bid", bid: -1 }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[1] }
  })
  await axios.post(`${apiRoot}/games/${gameId}/turn`, 
  { action: "Bid", bid: -1 }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[2] }
  })
  const final = await axios.post(`${apiRoot}/games/${gameId}/turn`, 
    { action: "Flip", targetPlayerIndex: 0 }, 
    { headers: { "X-OpenSkull-UserId": TEST_PLAYER_IDS[0] }
  })
  expect(final.data.gameComplete).toBe(true);
  expect(final.data.roundWinners.length).toBe(2);
  expect(final.data.roundWinners[0]).toBe(0);
  expect(final.data.roundWinners[0]).toBe(0);
})