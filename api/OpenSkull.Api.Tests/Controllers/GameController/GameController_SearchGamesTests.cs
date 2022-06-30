using OpenSkull.Api.Controllers;
using Moq;
using Microsoft.Extensions.Logging;
using OpenSkull.Api.Storage;
using OpenSkull.Api.Functions;
using Microsoft.AspNetCore.Mvc;
using OpenSkull.Api.DTO;
using Microsoft.AspNetCore.Http;
using OpenSkull.Api.Queue;

namespace OpenSkull.Api.Tests;

[TestClass]
public class GameController_SearchGamesTests {
  [TestMethod]
  public async Task SearchGames_NoParameters_GetsAllPlayersGames(){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    var testGame = GameFunctions.CreateNew(new Guid[3] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }).Value;
    var testGameStorage = new GameStorage {
      Game = testGame,
      Id = testGameId
    };
    mockGameStorage.Setup(x => x.SearchGames(It.IsAny<GameSearchParameters>())).ReturnsAsync(new GameStorage[] { testGameStorage });

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = testGame.PlayerIds[1].ToString();

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<IGameCreationQueue>().Object,
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
    var result = await gameController.SearchGames();

    // Assert
    mockGameStorage.Verify(m => m.SearchGames(It.IsAny<GameSearchParameters>()), Times.Once);
    Assert.AreEqual(1, result.Value!.Length);
    Assert.AreEqual(testGameId, result.Value![0].Id);
    Assert.AreEqual(1, (result.Value![0] as PlayerGame)!.PlayerIndex);
  }


  [TestMethod]
  public async Task SearchGames_Parameters_PassesToSearchWithoutPlayerId(){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    var testGame = GameFunctions.CreateNew(new Guid[3] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }).Value;
    var testGameStorage = new GameStorage {
      Game = testGame,
      Id = testGameId
    };
    mockGameStorage.Setup(x => x.SearchGames(It.IsAny<GameSearchParameters>())).ReturnsAsync(new GameStorage[] { testGameStorage });

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = testGame.PlayerIds[1].ToString();

    var searchId = testGame.PlayerIds[2];

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<IGameCreationQueue>().Object,
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
    var result = await gameController.SearchGames(new Guid[] { searchId });

    // Assert
    mockGameStorage.Verify(m => m.SearchGames(It.IsAny<GameSearchParameters>()), Times.Once);
    Assert.AreEqual(1, result.Value!.Length);
    Assert.AreEqual(testGameId, result.Value![0].Id);
    Assert.AreEqual(1, (result.Value![0] as PlayerGame)!.PlayerIndex);
  }
  
  [TestMethod]
  public async Task SearchGames_Parameters_PlayerIdNotOnGameReturnsPublicGame(){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    var testGame = GameFunctions.CreateNew(new Guid[3] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }).Value;
    var testGameStorage = new GameStorage {
      Game = testGame,
      Id = testGameId
    };
    mockGameStorage.Setup(x => x.SearchGames(It.IsAny<GameSearchParameters>())).ReturnsAsync(new GameStorage[] { testGameStorage });

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = Guid.NewGuid().ToString();

    var searchId = testGame.PlayerIds[2];

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<IGameCreationQueue>().Object,
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
    var result = await gameController.SearchGames(new Guid[] { searchId });

    // Assert
    mockGameStorage.Verify(m => m.SearchGames(It.IsAny<GameSearchParameters>()), Times.Once);
    Assert.AreEqual(1, result.Value!.Length);
    Assert.AreEqual(testGameId, result.Value![0].Id);
    Assert.IsNull((result.Value![0] as PlayerGame));
    Assert.IsNotNull((result.Value![0] as PublicGame));
  }
}