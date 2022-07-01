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
public class GameController_JoinQueue {
  [TestMethod]
  public async Task JoinQueue_HappyPath_NullQueueResult_ReturnsNoContent(){
    // Arrange
    var playerId = Guid.NewGuid();
    int gameSize = 3;

    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = playerId.ToString();
    var mockGameQueue = new Mock<IGameCreationQueue>();
    mockGameQueue.Setup(x => x.JoinGameQueue(playerId, gameSize)).ReturnsAsync(() => {
      return null;
    });
    
    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      new Mock<IGameStorage>().Object,
      mockGameQueue.Object,
      new Mock<IWebsocketManager>().Object,
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
    var result = await gameController.JoinQueue(new GameController.GameQueueParameters { GameSize = gameSize });

    // Assert
    Assert.AreEqual(typeof(NoContentResult), result.Result!.GetType());
    mockGameQueue.Verify(x => x.JoinGameQueue(playerId, gameSize), Times.Once);
  }

  [TestMethod]
  public async Task JoinQueue_HappyPath_GameQueueResult_ReturnsPlayerGame(){
    // Arrange
    var playerId = Guid.NewGuid();
    int gameSize = 3;
    
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = playerId.ToString();
    var mockGameQueue = new Mock<IGameCreationQueue>();
    mockGameQueue.Setup(x => x.JoinGameQueue(playerId, gameSize))
      .ReturnsAsync(new GameStorage{ 
        Id = Guid.NewGuid(), 
        Game = GameFunctions.CreateNew(new Guid[3] { playerId, Guid.NewGuid(), Guid.NewGuid() }).Value 
        });

    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      new Mock<IGameStorage>().Object,
      mockGameQueue.Object,
      new Mock<IWebsocketManager>().Object,
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
    var result = await gameController.JoinQueue(new GameController.GameQueueParameters { GameSize = gameSize });

    // Assert
    Assert.AreEqual(typeof(PlayerGame), result.Value!.GetType());
    mockGameQueue.Verify(x => x.JoinGameQueue(playerId, gameSize), Times.Once);
  }
}
