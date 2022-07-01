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

namespace OpenSkull.Api.Tests;

[TestClass]
public class GameController_GetGameTests {
  [TestMethod]
  public async Task GetGame_GameNotFound_Returns404(){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    mockGameStorage.Setup(x => x.GetGameById(testGameId)).ReturnsAsync(StorageError.NotFound);
    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<IGameCreationQueue>().Object,
      new Mock<IWebSocketManager>().Object,
      new Mock<GameCreateNew>().Object,
      new Mock<TurnPlayCard>().Object,
      new Mock<TurnPlaceBid>().Object,
      new Mock<TurnFlipCard>().Object
    );

    // Act
    var result = await gameController.GetGame(testGameId);

    // Assert
    Assert.AreEqual(typeof(NotFoundResult), result.Result!.GetType());
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
  }

  [TestMethod]
  public async Task GetGame_GameFound_NoUserId_ReturnsPublicGame(){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    var testGame = GameFunctions.CreateNew(new Guid[3] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }).Value;
    var testGameStorage = new GameStorage {
      Game = testGame,
      Id = testGameId
    };
    mockGameStorage.Setup(x => x.GetGameById(testGameId)).ReturnsAsync(testGameStorage);
    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<IGameCreationQueue>().Object,
      new Mock<IWebSocketManager>().Object,
      new Mock<GameCreateNew>().Object,
      new Mock<TurnPlayCard>().Object,
      new Mock<TurnPlaceBid>().Object,
      new Mock<TurnFlipCard>().Object
    );

    // Act
    var result = await gameController.GetGame(testGameId);

    // Assert
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
    Assert.AreEqual(typeof(PublicGame), result.Value!.GetType());
  }
  
  [TestMethod]
  public async Task GetGame_GameFound_UserIdNotInGame_ReturnsPublicGame(){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    var testGame = GameFunctions.CreateNew(new Guid[3] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }).Value;
    var testGameStorage = new GameStorage {
      Game = testGame,
      Id = testGameId
    };
    mockGameStorage.Setup(x => x.GetGameById(testGameId)).ReturnsAsync(testGameStorage);

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = Guid.NewGuid().ToString();

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<IGameCreationQueue>().Object,
      new Mock<IWebSocketManager>().Object,
      new Mock<GameCreateNew>().Object,
      new Mock<TurnPlayCard>().Object,
      new Mock<TurnPlaceBid>().Object,
      new Mock<TurnFlipCard>().Object
    ){ 
      ControllerContext = new ControllerContext()
      {
          HttpContext = httpContext
      }
    };

    // Act
    var result = await gameController.GetGame(testGameId);

    // Assert
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
    Assert.AreEqual(typeof(PublicGame), result.Value!.GetType());
  }

  [TestMethod]
  public async Task GetGame_GameFound_UserIdInGame_ReturnsPlayerGame(){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    var testGame = GameFunctions.CreateNew(new Guid[3] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }).Value;
    var testGameStorage = new GameStorage {
      Game = testGame,
      Id = testGameId
    };
    mockGameStorage.Setup(x => x.GetGameById(testGameId)).ReturnsAsync(testGameStorage);

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = testGame.PlayerIds[1].ToString();

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<IGameCreationQueue>().Object,
      new Mock<IWebSocketManager>().Object,
      new Mock<GameCreateNew>().Object,
      new Mock<TurnPlayCard>().Object,
      new Mock<TurnPlaceBid>().Object,
      new Mock<TurnFlipCard>().Object
    ){ 
      ControllerContext = new ControllerContext()
      {
          HttpContext = httpContext
      }
    };

    // Act
    var result = await gameController.GetGame(testGameId);

    // Assert
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
    Assert.AreEqual(typeof(PlayerGame), result.Value!.GetType());
    var gameValue = result.Value! as PlayerGame;
    Assert.AreEqual(1, gameValue!.PlayerIndex);
    Assert.AreEqual(testGame.PlayerIds[1], gameValue!.PlayerId);
  }
}
