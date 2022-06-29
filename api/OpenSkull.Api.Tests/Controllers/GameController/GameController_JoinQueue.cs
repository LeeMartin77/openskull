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
public class GameController_JoinQueue {
  [TestMethod]
  public async Task JoinQueue_HappyPath_AllRuns(){
    // Arrange
    var playerId = Guid.NewGuid();
    
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = playerId.ToString();
    
    var gameController = new GameController(
      new Mock<ILogger<GameController>>().Object,
      new Mock<IGameStorage>().Object,
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
    var result = await gameController.JoinQueue(new GameController.GameQueueParameters { GameSize = 3 });

    // Assert
    Assert.AreEqual(typeof(NoContentResult), result.Result!.GetType());
  }
}
