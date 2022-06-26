using OpenSkull.Api.Controllers;
using Moq;
using Microsoft.Extensions.Logging;
using OpenSkull.Api.Storage;
using OpenSkull.Api.Functions;
using Microsoft.AspNetCore.Mvc;

namespace OpenSkull.Api.Tests;

[TestClass]
public class GameControllerTests {
  [TestMethod]
  public async Task GameNotFound_Returns404(){
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
}