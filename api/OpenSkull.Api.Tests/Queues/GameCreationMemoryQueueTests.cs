using OpenSkull.Api.Functions;
using OpenSkull.Api.Storage;
using OpenSkull.Api.Queue;
using Moq;
using OpenSkull.Api.DTO;
using OpenSkull.Api.Messaging;

namespace OpenSkull.Api.Tests;

[TestClass]
public class GameCreationMemoryQueueTests 
{
  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public async Task HappyPath_GameCreationMemoryQueue_CreatesGamesOnFinalPlayerOnGivenQueue(int playerCount) {
    // Arrange
    Guid[] testPlayerIds = new Guid[playerCount];
    for(int i = 0; i < playerCount; i++) {
      testPlayerIds[i] = Guid.NewGuid();
    }

    var createdGame = new Game {
      ActivePlayerIndex = 999
    };
    var gameCreationFunction = new Mock<GameCreateNew>();
    gameCreationFunction.Setup(x => x(It.IsAny<Guid[]>())).Returns(createdGame);

    var gameStorage = new GameStorage {
      Id = Guid.NewGuid(),
      Game = new Game {
        PlayerIds = testPlayerIds
      }
    };
    var mockStorage = new Mock<IGameStorage>();
    mockStorage.Setup(x => x.StoreNewGame(createdGame)).ReturnsAsync(gameStorage);

    var mockWebsocketManager = new Mock<IWebSocketManager>();

    var creationQueue = new GameCreationMemoryQueue(
      gameCreationFunction.Object,
      mockStorage.Object,
      mockWebsocketManager.Object
    );

    // Act
    var queueResults = new List<bool>();
    for(int i = 0; i < playerCount; i++) {
      queueResults.Add((await creationQueue.JoinGameQueue(testPlayerIds[i], playerCount)).Value);
    }

    // Assert
    Assert.AreEqual(playerCount, queueResults.Count());
    mockStorage.Verify(x => x.StoreNewGame(It.IsAny<Game>()), Times.Once);
    foreach (var playerId in testPlayerIds) {
      mockWebsocketManager.Verify(x => x.BroadcastToConnectedWebsockets(WebSocketType.Player, playerId, new OpenskullMessage { Id = gameStorage.Id, Activity = "GameCreated" }));
    }
  }
}