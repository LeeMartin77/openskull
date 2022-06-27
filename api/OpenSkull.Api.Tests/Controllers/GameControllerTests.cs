using OpenSkull.Api.Controllers;
using Moq;
using Microsoft.Extensions.Logging;
using OpenSkull.Api.Storage;
using OpenSkull.Api.Functions;
using Microsoft.AspNetCore.Mvc;
using OpenSkull.Api.DTO;
using Microsoft.AspNetCore.Http;

namespace OpenSkull.Api.Tests;

[TestClass]
public class GameControllerTests {
  [TestMethod]
  public async Task GetGame_GameNotFound_Returns404(){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    mockGameStorage.Setup(x => x.GetGameById(testGameId)).ReturnsAsync(StorageError.NotFound);
    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
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

  [TestMethod]
  public async Task PlayGameTurn_PlayerIdNotSet_Returns400(){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    
    var httpContext = new DefaultHttpContext();
    
    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
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
    var result = await gameController.PlayGameTurn(testGameId);

    // Assert
    Assert.AreEqual(typeof(BadRequestResult), result.Result!.GetType());
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Never);
  }

  [TestMethod]
  public async Task PlayGameTurn_BodyIsNull_Returns400(){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = Guid.NewGuid().ToString();

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
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
    var result = await gameController.PlayGameTurn(testGameId, null);

    // Assert
    Assert.AreEqual(typeof(BadRequestResult), result.Result!.GetType());
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Never);
  }

  [TestMethod]
  public async Task PlayGameTurn_GameNotFound_Returns404(){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    mockGameStorage.Setup(x => x.GetGameById(testGameId)).ReturnsAsync(StorageError.NotFound);

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = Guid.NewGuid().ToString();

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
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
    var result = await gameController.PlayGameTurn(testGameId, new PlayCardTurnInputs{ Action = GameController.ActionStrings[0] });

    // Assert
    Assert.AreEqual(typeof(NotFoundResult), result.Result!.GetType());
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
  }

  [TestMethod]
  public async Task PlayGameTurn_PlayerIdNotInGame_Returns403(){
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
    var result = await gameController.PlayGameTurn(testGameId, new PlayCardTurnInputs{ Action = GameController.ActionStrings[0] });

    // Assert
    Assert.AreEqual(typeof(ForbidResult), result.Result!.GetType());
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
  }

  [TestMethod]
  public async Task PlayGameTurn_ActionNotRecognised_Returns400(){
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
    var result = await gameController.PlayGameTurn(testGameId, new PlayCardTurnInputs{ Action = "SomethingNotValid" });

    // Assert
    Assert.AreEqual(typeof(BadRequestResult), result.Result!.GetType());
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Never);
  }

  
  [TestMethod]
  [DataRow(GameTurnError.InvalidCardId)]
  [DataRow(GameTurnError.InvalidPlayerId)]
  [DataRow(GameTurnError.CannotPlayCardAfterBid)]
  public async Task PlayGameTurn_PlayCardAction_ErrorResult_Returns400AndDoesNotUpdateGame(GameTurnError err){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    var testGame = GameFunctions.CreateNew(new Guid[3] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }).Value;
    var testGameStorage = new GameStorage {
      Game = testGame,
      Id = testGameId
    };
    mockGameStorage.Setup(x => x.GetGameById(testGameId)).ReturnsAsync(testGameStorage);

    var cardIdInput = Guid.NewGuid();

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = testGame.PlayerIds[1].ToString();

    var mockPlayCardTurn = new Mock<TurnPlayCard>();
    mockPlayCardTurn.Setup(x=> x(testGame, testGame.PlayerIds[1], cardIdInput)).Returns(err);

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<GameCreateNew>().Object,
      mockPlayCardTurn.Object,
      new Mock<TurnPlaceBid>().Object,
      new Mock<TurnFlipCard>().Object
    ){ 
      ControllerContext = new ControllerContext()
      {
          HttpContext = httpContext
      }
    };

    // Act
    var result = await gameController.PlayGameTurn(testGameId, new PlayCardTurnInputs{ Action = GameController.ActionStrings[0], CardId = cardIdInput });

    // Assert
    Assert.AreEqual(typeof(BadRequestObjectResult), result.Result!.GetType());
    Assert.AreEqual(GameFunctions.GameTurnErrorMessage[(int)err], (result.Result as BadRequestObjectResult)!.Value);
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
    mockPlayCardTurn.Verify(x => x(testGame, testGame.PlayerIds[1], cardIdInput), Times.Once);
    mockGameStorage.Verify(m => m.UpdateGame(It.IsAny<GameStorage>()), Times.Never);
  }

  [TestMethod]
  [DataRow(StorageError.VersionMismatch, 409)]
  [DataRow(StorageError.CantStore, 500)]
  public async Task PlayGameTurn_PlayCardAction_SuccessResult_SaveError_ReturnsError(StorageError err, int expectedStatusCode){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    var testGame = GameFunctions.CreateNew(new Guid[3] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }).Value;
    var testGameStorage = new GameStorage {
      Game = testGame,
      Id = testGameId,
      VersionTag = Guid.NewGuid().ToString()
    };
    mockGameStorage.Setup(x => x.GetGameById(testGameId)).ReturnsAsync(testGameStorage);

    var cardIdInput = Guid.NewGuid();

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = testGame.PlayerIds[1].ToString();

    var actionGame = testGame with { ActivePlayerIndex = 999 };
    var mockPlayCardTurn = new Mock<TurnPlayCard>();
    mockPlayCardTurn.Setup(x=> x(testGame, testGame.PlayerIds[1], cardIdInput)).Returns(actionGame);

    mockGameStorage.Setup(x => x.UpdateGame(It.IsAny<GameStorage>())).ReturnsAsync(err);

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<GameCreateNew>().Object,
      mockPlayCardTurn.Object,
      new Mock<TurnPlaceBid>().Object,
      new Mock<TurnFlipCard>().Object
    ){ 
      ControllerContext = new ControllerContext()
      {
          HttpContext = httpContext
      }
    };

    // Act
    var result = await gameController.PlayGameTurn(testGameId, new PlayCardTurnInputs{ Action = GameController.ActionStrings[0], CardId = cardIdInput });

    // Assert
    Assert.AreEqual(expectedStatusCode, (result.Result as ObjectResult)!.StatusCode);
    Assert.AreEqual(StorageErrorMessages.StringValues[(int)err], (result.Result as ObjectResult)!.Value);
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
    mockPlayCardTurn.Verify(x => x(testGame, testGame.PlayerIds[1], cardIdInput), Times.Once);
    mockGameStorage.Verify(m => m.UpdateGame(It.IsAny<GameStorage>()), Times.Once);
  }

  [TestMethod]
  public async Task HappyPath_PlayGameTurn_PlayCardAction_SuccessResult_Saves_ReturnsPlayerViewOfGame(){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    var testGame = GameFunctions.CreateNew(new Guid[3] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }).Value;
    var testGameStorage = new GameStorage {
      Game = testGame,
      Id = testGameId,
      VersionTag = Guid.NewGuid().ToString()
    };
    mockGameStorage.Setup(x => x.GetGameById(testGameId)).ReturnsAsync(testGameStorage);

    var cardIdInput = Guid.NewGuid();

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = testGame.PlayerIds[1].ToString();

    var actionGame = testGame with { ActivePlayerIndex = 999 };
    var mockPlayCardTurn = new Mock<TurnPlayCard>();
    mockPlayCardTurn.Setup(x=> x(testGame, testGame.PlayerIds[1], cardIdInput)).Returns(actionGame);

    mockGameStorage.Setup(x => x.UpdateGame(It.IsAny<GameStorage>())).ReturnsAsync(testGameStorage with {
      Game = actionGame,
      VersionTag = Guid.NewGuid().ToString()
    });

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<GameCreateNew>().Object,
      mockPlayCardTurn.Object,
      new Mock<TurnPlaceBid>().Object,
      new Mock<TurnFlipCard>().Object
    ){ 
      ControllerContext = new ControllerContext()
      {
          HttpContext = httpContext
      }
    };

    // Act
    var result = await gameController.PlayGameTurn(testGameId, new PlayCardTurnInputs{ Action = GameController.ActionStrings[0], CardId = cardIdInput });

    // Assert
    Assert.AreEqual(typeof(PlayerGame), result.Value!.GetType());
    var gameValue = result.Value! as PlayerGame;
    Assert.AreEqual(1, gameValue!.PlayerIndex);
    Assert.AreEqual(actionGame.PlayerIds[1], gameValue!.PlayerId);
    Assert.AreEqual(actionGame.ActivePlayerIndex, gameValue!.ActivePlayerIndex);
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
    mockPlayCardTurn.Verify(x => x(testGame, testGame.PlayerIds[1], cardIdInput), Times.Once);
    mockGameStorage.Verify(m => m.UpdateGame(It.IsAny<GameStorage>()), Times.Once);
  }
}