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
    var result = await gameController.PlayGameTurn(testGameId, new PlayTurnInputs{ Action = TurnAction.Card.ToString(), CardId = Guid.NewGuid() });

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
    var result = await gameController.PlayGameTurn(testGameId, new PlayTurnInputs{ Action = TurnAction.Card.ToString(), CardId = Guid.NewGuid() });

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
    var result = await gameController.PlayGameTurn(testGameId, new PlayTurnInputs{ Action = "SomethingNotValid" });

    // Assert
    Assert.AreEqual(typeof(BadRequestResult), result.Result!.GetType());
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Never);
  }

  [TestMethod]
  [DataRow(TurnAction.Card)]
  [DataRow(TurnAction.Bid)]
  [DataRow(TurnAction.Flip)]
  public async Task PlayGameTurn_InputNotGiven_Returns400(TurnAction action){
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

    var input = new PlayTurnInputs
    { 
      Action = action.ToString(),
      CardId = action == TurnAction.Card ? null : Guid.NewGuid(),
      Bid = action == TurnAction.Bid ? null : 0,
      TargetPlayerIndex = action == TurnAction.Flip ? null : 0
    };

    // Act
    var result = await gameController.PlayGameTurn(testGameId, input);

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
    var result = await gameController.PlayGameTurn(testGameId, new PlayTurnInputs{ Action = TurnAction.Card.ToString(), CardId = cardIdInput });

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
    var result = await gameController.PlayGameTurn(testGameId, new PlayTurnInputs{ Action = TurnAction.Card.ToString(), CardId = cardIdInput });

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
    var result = await gameController.PlayGameTurn(testGameId, new PlayTurnInputs{ Action = TurnAction.Card.ToString(), CardId = cardIdInput });

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


  [TestMethod]
  [DataRow(GameTurnError.InvalidCardId)]
  [DataRow(GameTurnError.InvalidPlayerId)]
  [DataRow(GameTurnError.CannotPlayCardAfterBid)]
  public async Task PlayGameTurn_PlaceBidAction_ErrorResult_Returns400AndDoesNotUpdateGame(GameTurnError err){
    // Arrange
    var testGameId = Guid.NewGuid();
    var mockGameStorage = new Mock<IGameStorage>();
    var testGame = GameFunctions.CreateNew(new Guid[3] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }).Value;
    var testGameStorage = new GameStorage {
      Game = testGame,
      Id = testGameId
    };
    mockGameStorage.Setup(x => x.GetGameById(testGameId)).ReturnsAsync(testGameStorage);

    var bidInput = 99;

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = testGame.PlayerIds[1].ToString();

    var mockPlaceBid = new Mock<TurnPlaceBid>();
    mockPlaceBid.Setup(x=> x(testGame, testGame.PlayerIds[1], bidInput)).Returns(err);

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<GameCreateNew>().Object,
      new Mock<TurnPlayCard>().Object,
      mockPlaceBid.Object,
      new Mock<TurnFlipCard>().Object
    ){ 
      ControllerContext = new ControllerContext()
      {
          HttpContext = httpContext
      }
    };

    // Act
    var result = await gameController.PlayGameTurn(testGameId, new PlayTurnInputs{ Action = TurnAction.Bid.ToString(), Bid = bidInput });

    // Assert
    Assert.AreEqual(typeof(BadRequestObjectResult), result.Result!.GetType());
    Assert.AreEqual(GameFunctions.GameTurnErrorMessage[(int)err], (result.Result as BadRequestObjectResult)!.Value);
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
    mockPlaceBid.Verify(x => x(testGame, testGame.PlayerIds[1], bidInput), Times.Once);
    mockGameStorage.Verify(m => m.UpdateGame(It.IsAny<GameStorage>()), Times.Never);
  }

  [TestMethod]
  [DataRow(StorageError.VersionMismatch, 409)]
  [DataRow(StorageError.CantStore, 500)]
  public async Task PlayGameTurn_PlaceBidAction_SuccessResult_SaveError_ReturnsError(StorageError err, int expectedStatusCode){
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

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = testGame.PlayerIds[1].ToString();

    var actionGame = testGame with { ActivePlayerIndex = 999 };

    var bidInput = 99;

    var mockPlaceBid = new Mock<TurnPlaceBid>();
    mockPlaceBid.Setup(x=> x(testGame, testGame.PlayerIds[1], bidInput)).Returns(actionGame);

    mockGameStorage.Setup(x => x.UpdateGame(It.IsAny<GameStorage>())).ReturnsAsync(err);

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<GameCreateNew>().Object,
      new Mock<TurnPlayCard>().Object,
      mockPlaceBid.Object,
      new Mock<TurnFlipCard>().Object
    ){ 
      ControllerContext = new ControllerContext()
      {
          HttpContext = httpContext
      }
    };

    // Act
    var result = await gameController.PlayGameTurn(testGameId, new PlayTurnInputs{ Action = TurnAction.Bid.ToString(), Bid = bidInput });

    // Assert
    Assert.AreEqual(expectedStatusCode, (result.Result as ObjectResult)!.StatusCode);
    Assert.AreEqual(StorageErrorMessages.StringValues[(int)err], (result.Result as ObjectResult)!.Value);
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
    mockPlaceBid.Verify(x => x(testGame, testGame.PlayerIds[1], bidInput), Times.Once);
    mockGameStorage.Verify(m => m.UpdateGame(It.IsAny<GameStorage>()), Times.Once);
  }

  [TestMethod]
  public async Task HappyPath_PlayGameTurn_PlaceBidAction_SuccessResult_Saves_ReturnsPlayerViewOfGame(){
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

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = testGame.PlayerIds[1].ToString();

    var actionGame = testGame with { ActivePlayerIndex = 999 };

    var bidInput = 99;

    var mockPlaceBid = new Mock<TurnPlaceBid>();
    mockPlaceBid.Setup(x=> x(testGame, testGame.PlayerIds[1], bidInput)).Returns(actionGame);

    mockGameStorage.Setup(x => x.UpdateGame(It.IsAny<GameStorage>())).ReturnsAsync(testGameStorage with {
      Game = actionGame,
      VersionTag = Guid.NewGuid().ToString()
    });

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<GameCreateNew>().Object,
      new Mock<TurnPlayCard>().Object,
      mockPlaceBid.Object,
      new Mock<TurnFlipCard>().Object
    ){ 
      ControllerContext = new ControllerContext()
      {
          HttpContext = httpContext
      }
    };

    // Act
    var result = await gameController.PlayGameTurn(testGameId, new PlayTurnInputs{ Action = TurnAction.Bid.ToString(), Bid = bidInput });

    // Assert
    Assert.AreEqual(typeof(PlayerGame), result.Value!.GetType());
    var gameValue = result.Value! as PlayerGame;
    Assert.AreEqual(1, gameValue!.PlayerIndex);
    Assert.AreEqual(actionGame.PlayerIds[1], gameValue!.PlayerId);
    Assert.AreEqual(actionGame.ActivePlayerIndex, gameValue!.ActivePlayerIndex);
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
    mockPlaceBid.Verify(x => x(testGame, testGame.PlayerIds[1], bidInput), Times.Once);
    mockGameStorage.Verify(m => m.UpdateGame(It.IsAny<GameStorage>()), Times.Once);
  }


  [TestMethod]
  [DataRow(GameTurnError.InvalidCardId)]
  [DataRow(GameTurnError.InvalidPlayerId)]
  [DataRow(GameTurnError.CannotPlayCardAfterBid)]
  public async Task PlayGameTurn_FlipCardAction_ErrorResult_Returns400AndDoesNotUpdateGame(GameTurnError err){
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

    var flipInput = 99;

    var mockFlipCard = new Mock<TurnFlipCard>();
    mockFlipCard.Setup(x=> x(testGame, testGame.PlayerIds[1], flipInput)).Returns(err);

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<GameCreateNew>().Object,
      new Mock<TurnPlayCard>().Object,
      new Mock<TurnPlaceBid>().Object,
      mockFlipCard.Object
    ){ 
      ControllerContext = new ControllerContext()
      {
          HttpContext = httpContext
      }
    };

    // Act
    var result = await gameController.PlayGameTurn(testGameId, new PlayTurnInputs{ Action = TurnAction.Flip.ToString(), TargetPlayerIndex = flipInput });

    // Assert
    Assert.AreEqual(typeof(BadRequestObjectResult), result.Result!.GetType());
    Assert.AreEqual(GameFunctions.GameTurnErrorMessage[(int)err], (result.Result as BadRequestObjectResult)!.Value);
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
    mockFlipCard.Verify(x => x(testGame, testGame.PlayerIds[1], flipInput), Times.Once);
    mockGameStorage.Verify(m => m.UpdateGame(It.IsAny<GameStorage>()), Times.Never);
  }

  [TestMethod]
  [DataRow(StorageError.VersionMismatch, 409)]
  [DataRow(StorageError.CantStore, 500)]
  public async Task PlayGameTurn_FlipCardAction_SuccessResult_SaveError_ReturnsError(StorageError err, int expectedStatusCode){
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

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = testGame.PlayerIds[1].ToString();

    var actionGame = testGame with { ActivePlayerIndex = 999 };

    var flipInput = 99;

    var mockFlipCard = new Mock<TurnFlipCard>();
    mockFlipCard.Setup(x=> x(testGame, testGame.PlayerIds[1], flipInput)).Returns(actionGame);

    mockGameStorage.Setup(x => x.UpdateGame(It.IsAny<GameStorage>())).ReturnsAsync(err);

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<GameCreateNew>().Object,
      new Mock<TurnPlayCard>().Object,
      new Mock<TurnPlaceBid>().Object,
      mockFlipCard.Object
    ){ 
      ControllerContext = new ControllerContext()
      {
          HttpContext = httpContext
      }
    };

    // Act
    var result = await gameController.PlayGameTurn(testGameId, new PlayTurnInputs{ Action = TurnAction.Flip.ToString(), TargetPlayerIndex = flipInput });

    // Assert
    Assert.AreEqual(expectedStatusCode, (result.Result as ObjectResult)!.StatusCode);
    Assert.AreEqual(StorageErrorMessages.StringValues[(int)err], (result.Result as ObjectResult)!.Value);
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
    mockFlipCard.Verify(x => x(testGame, testGame.PlayerIds[1], flipInput), Times.Once);
    mockGameStorage.Verify(m => m.UpdateGame(It.IsAny<GameStorage>()), Times.Once);
  }

  [TestMethod]
  public async Task HappyPath_PlayGameTurn_FlipCardAction_SuccessResult_Saves_ReturnsPlayerViewOfGame(){
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

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = testGame.PlayerIds[1].ToString();

    var actionGame = testGame with { ActivePlayerIndex = 999 };

    var flipInput = 99;

    var mockFlipCard = new Mock<TurnFlipCard>();
    mockFlipCard.Setup(x=> x(testGame, testGame.PlayerIds[1], flipInput)).Returns(actionGame);

    mockGameStorage.Setup(x => x.UpdateGame(It.IsAny<GameStorage>())).ReturnsAsync(testGameStorage with {
      Game = actionGame,
      VersionTag = Guid.NewGuid().ToString()
    });

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      mockGameStorage.Object,
      new Mock<GameCreateNew>().Object,
      new Mock<TurnPlayCard>().Object,
      new Mock<TurnPlaceBid>().Object,
      mockFlipCard.Object
    ){ 
      ControllerContext = new ControllerContext()
      {
          HttpContext = httpContext
      }
    };

    // Act
    var result = await gameController.PlayGameTurn(testGameId, new PlayTurnInputs{ Action = TurnAction.Flip.ToString(), TargetPlayerIndex = flipInput });

    // Assert
    Assert.AreEqual(typeof(PlayerGame), result.Value!.GetType());
    var gameValue = result.Value! as PlayerGame;
    Assert.AreEqual(1, gameValue!.PlayerIndex);
    Assert.AreEqual(actionGame.PlayerIds[1], gameValue!.PlayerId);
    Assert.AreEqual(actionGame.ActivePlayerIndex, gameValue!.ActivePlayerIndex);
    mockGameStorage.Verify(m => m.GetGameById(testGameId), Times.Once);
    mockFlipCard.Verify(x => x(testGame, testGame.PlayerIds[1], flipInput), Times.Once);
    mockGameStorage.Verify(m => m.UpdateGame(It.IsAny<GameStorage>()), Times.Once);
  }
}