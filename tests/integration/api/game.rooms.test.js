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

describe ("Room Tests", () => {
  let TEST_PLAYER_IDS;
  let connections = [];
  beforeEach(() => {
    TEST_PLAYER_IDS = [
      [crypto.randomUUID(), crypto.randomUUID(), "playerOne"],
      [crypto.randomUUID(), crypto.randomUUID(), "playerTwo"],
      [crypto.randomUUID(), crypto.randomUUID(), "playerThree"]
    ]
  });
  
  afterEach(async () => {
    await Promise.all(connections.map(async c =>  c.state == "Connected" && await c.stop()))
  });
  
  test("Room :: Can connect to room and see response", async () => {  
    const ROOM_ID = crypto.randomUUID();
  
    const messages = [
      [],
      [],
      []
    ]
  
    const responseHandler = (index, msg, msgs = messages) => {
      msgs[index].push(msg)
    }
  
    connections = await Promise.all(TEST_PLAYER_IDS.map(async (id, i) => {
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
  
      await connection.send("updateNickname", id[0], id[1], id[2])
  
      await new Promise(resolve => setTimeout(resolve, 100))
  
      return connection;
    }));
    
    await connections[0].send("joinRoom", TEST_PLAYER_IDS[0][0], TEST_PLAYER_IDS[0][1], ROOM_ID)
  
    while(!messages[0].map(x => x.activity).includes("RoomUpdate")) {
      await new Promise(resolve => setTimeout(resolve, 10))
    }
  
    const updateMessage = messages[0].find(x => x.activity == "RoomUpdate");
  
    expect(updateMessage.roomDetails.roomId).toBe(ROOM_ID)
    expect(updateMessage.roomDetails.playerNicknames.length).toBe(1)
    expect(updateMessage.roomDetails.playerNicknames[0]).toBe("playerOne")
  
  });
  
  test("Room :: Players Joining and Leaving can see", async () => {  
    const ROOM_ID = crypto.randomUUID();
  
    const messages = [
      [],
      [],
      []
    ]
  
    const responseHandler = (index, msg, msgs = messages) => {
      msgs[index].push(msg)
    }
  
    connections = await Promise.all(TEST_PLAYER_IDS.map(async (id, i) => {
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
  
      await connection.send("updateNickname", id[0], id[1], id[2])
  
      await new Promise(resolve => setTimeout(resolve, 100))
  
      return connection;
    }));
    
    await connections[0].send("joinRoom", TEST_PLAYER_IDS[0][0], TEST_PLAYER_IDS[0][1], ROOM_ID)
  
    while(!messages[0].map(x => x.activity).includes("RoomUpdate")) {
      await new Promise(resolve => setTimeout(resolve, 10))
    }
  
    const updateMessage = messages[0].find(x => x.activity == "RoomUpdate");
  
    expect(updateMessage.roomDetails.roomId).toBe(ROOM_ID)
    expect(updateMessage.roomDetails.playerNicknames.length).toBe(1)
    expect(updateMessage.roomDetails.playerNicknames[0]).toBe("playerOne")

    await connections[1].send("joinRoom", TEST_PLAYER_IDS[1][0], TEST_PLAYER_IDS[1][1], ROOM_ID)
  
    while(!messages[1].map(x => x.activity).includes("RoomUpdate")) {
      await new Promise(resolve => setTimeout(resolve, 10))
    }

    const firstPlayerUpdate = messages[0].filter(x => x.activity == "RoomUpdate")[1];
    const secondPlayerUpdate = messages[1].find(x => x.activity == "RoomUpdate");

    expect(firstPlayerUpdate.roomDetails.roomId).toBe(ROOM_ID)
    expect(firstPlayerUpdate.roomDetails.playerNicknames.length).toBe(2)
    expect(firstPlayerUpdate.roomDetails.playerNicknames).toContain("playerOne")
    expect(firstPlayerUpdate.roomDetails.playerNicknames).toContain("playerTwo")

    expect(secondPlayerUpdate.roomDetails.roomId).toBe(ROOM_ID)
    expect(secondPlayerUpdate.roomDetails.playerNicknames.length).toBe(2)
    expect(secondPlayerUpdate.roomDetails.playerNicknames).toContain("playerOne")
    expect(secondPlayerUpdate.roomDetails.playerNicknames).toContain("playerTwo")


    await connections[1].send("leaveRoom", TEST_PLAYER_IDS[0][0], TEST_PLAYER_IDS[0][1], ROOM_ID)

    while(messages[1].map(x => x.activity).filter(x => x == "RoomUpdate").length < 2) {
      await new Promise(resolve => setTimeout(resolve, 10))
    }

    const secondPlayerUpdate2 = messages[1].filter(x => x.activity == "RoomUpdate")[1];
    expect(messages[0].filter(x => x.activity == "RoomUpdate").length).toBe(2);
    expect(secondPlayerUpdate2.roomDetails.roomId).toBe(ROOM_ID)
    expect(secondPlayerUpdate2.roomDetails.playerNicknames.length).toBe(1)
    expect(secondPlayerUpdate2.roomDetails.playerNicknames).toContain("playerTwo")
  });

  // This is pretty flakey, probably because of signalr not being
  // hasty about calling a connection dead.
  test.skip("Room :: Player disconnecting leaves room", async () => {  
    const ROOM_ID = crypto.randomUUID();
  
    const messages = [
      [],
      [],
      []
    ]
  
    const responseHandler = (index, msg, msgs = messages) => {
      msgs[index].push(msg)
    }
  
    connections = await Promise.all(TEST_PLAYER_IDS.map(async (id, i) => {
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
  
      await connection.send("updateNickname", id[0], id[1], id[2])
  
      await new Promise(resolve => setTimeout(resolve, 100))
  
      return connection;
    }));
    
    await connections[0].send("joinRoom", TEST_PLAYER_IDS[0][0], TEST_PLAYER_IDS[0][1], ROOM_ID)
  
    await connections[1].send("joinRoom", TEST_PLAYER_IDS[1][0], TEST_PLAYER_IDS[1][1], ROOM_ID)
  
    while(!messages[1].map(x => x.activity).includes("RoomUpdate")) {
      await new Promise(resolve => setTimeout(resolve, 10))
    }

    await connections[0].stop();

    while(messages[1].map(x => x.activity).filter(x => x == "RoomUpdate").length < 2) {
      await new Promise(resolve => setTimeout(resolve, 10))
    }

    const secondPlayerUpdate2 = messages[1].filter(x => x.activity == "RoomUpdate")[1];
    expect(messages[0].filter(x => x.activity == "RoomUpdate").length).toBe(2);
    expect(secondPlayerUpdate2.roomDetails.roomId).toBe(ROOM_ID)
    expect(secondPlayerUpdate2.roomDetails.playerNicknames.length).toBe(1)
    expect(secondPlayerUpdate2.roomDetails.playerNicknames).toContain("playerTwo")
  });


  test("Room :: Can create game from room of players", async () => {  
    const ROOM_ID = crypto.randomUUID();
  
    const messages = [
      [],
      [],
      []
    ]
  
    const responseHandler = (index, msg, msgs = messages) => {
      msgs[index].push(msg)
    }
  
    connections = await Promise.all(TEST_PLAYER_IDS.map(async (id, i) => {
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
  
      await connection.send("updateNickname", id[0], id[1], id[2])
  
      await new Promise(resolve => setTimeout(resolve, 100))
  
      await connection.send("joinRoom", id[0], id[1], ROOM_ID)
  
      while(!messages[i].map(x => x.activity).includes("RoomUpdate")) {
        await new Promise(resolve => setTimeout(resolve, 10))
      }

      return connection;
    }));

    await connections[0].send("createRoomGame", TEST_PLAYER_IDS[0][0], TEST_PLAYER_IDS[0][1], ROOM_ID)

    while(!messages.every(msgs => msgs.map(x => x.activity).includes("GameCreated")))
    {
      await new Promise(resolve => setTimeout(resolve, 10))
    }

    const gameCreationMessage = messages[0].find(x => x.activity == "GameCreated");

    expect(messages.every(msgs => msgs.findIndex(x => x.activity == "GameCreated" && x.id == gameCreationMessage.id) != -1)).toBe(true);

    const gameData = await axios.get(`${apiRoot}/games/${gameCreationMessage.id}`,
    {
      headers: {
        "X-OpenSkull-UserId": TEST_PLAYER_IDS[0][0],
        "X-OpenSkull-UserSecret": TEST_PLAYER_IDS[0][1]
      }
    })

    expect(gameData.status).toBe(200);
  });
})
