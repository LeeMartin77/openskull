using OpenSkull.Api.Controllers;
using Moq;
using Microsoft.Extensions.Logging;
using OpenSkull.Api.Storage;
using OpenSkull.Api.Functions;
using Microsoft.AspNetCore.Mvc;
using OpenSkull.Api.DTO;
using Microsoft.AspNetCore.Http;
using OpenSkull.Api.Queue;
using OpenSkull.Api.Messaging;
using OpenSkull.Api.Middleware;

namespace OpenSkull.Api.Tests;

[TestClass]
public class GameController_CreateGameTests {
  [TestMethod]
  public async Task GetGame_CreateGameHappyPath_ReturnsGameId(){
    // Arrange
    var mockGameStorage = new Mock<IGameStorage>();
    var mockCreateGame = new Mock<GameCreateNew>();

    var testPlayerIds = new List<Guid> {
      Guid.NewGuid(),
      Guid.NewGuid(),
      Guid.NewGuid()
    };

    var createdGame = new Game();

    var createGameInput = new CreateGameInput {
      PlayerIds = testPlayerIds
    };

    var mockStoredGameId = Guid.NewGuid();

    mockCreateGame.Setup(x => x(createGameInput.PlayerIds.ToArray())).Returns(createdGame);

    mockGameStorage.Setup(x => x.StoreNewGame(createdGame)).ReturnsAsync(new GameStorage { Id = mockStoredGameId });
    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<IPlayerStorage>().Object,
      new Mock<IGameCreationQueue>().Object,
      new Mock<IWebSocketManager>().Object,
      mockCreateGame.Object,
      new Mock<TurnPlayCard>().Object,
      new Mock<TurnPlaceBid>().Object,
      new Mock<TurnFlipCard>().Object
    );

    // Act
    var result = await gameController.CreateGame(createGameInput);

    // Assert
    Assert.AreEqual(typeof(Guid), result.Value!.GetType());
    Assert.AreEqual(mockStoredGameId, result.Value!);
  }
}
