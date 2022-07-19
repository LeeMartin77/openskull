using OpenSkull.Api.Middleware;
using OpenSkull.Api.Storage;
using Moq;
using Microsoft.AspNetCore.Http;

namespace OpenSkull.Api.Tests;

[TestClass]
public class VerifyPlayerMiddlewareTests
{
  [TestMethod]
  public async Task HappyPath_ValidUser_SetsContextAndCallsNext()
  {
    // Arrange
    Guid playerId = Guid.NewGuid();
    string secret = Guid.NewGuid().ToString();
    string salt = Guid.NewGuid().ToString();
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = playerId.ToString();
    httpContext.Request.Headers["X-OpenSkull-UserSecret"] = secret;

    var mockStorage = new Mock<IPlayerStorage>();
    mockStorage.Setup(x => x.GetPlayerById(playerId))
      .ReturnsAsync(new Player {
        Id = playerId,
        HashedSecret = VerifyPlayerMiddleware.HashSecret(secret, salt),
        Salt = salt
      });

    var mockDelegate = new Mock<RequestDelegate>();
    HttpContext? nextContext = null;
    mockDelegate.Setup(x => x(It.IsAny<HttpContext>()))
      .Callback<HttpContext>((con) => nextContext = con);

    var middleware = new VerifyPlayerMiddleware(mockDelegate.Object, mockStorage.Object, null);
    // Act
    await middleware.InvokeAsync(httpContext);

    // Assert
    mockDelegate.Verify(x => x(httpContext), Times.Once);
    mockStorage.Verify(x => x.GetPlayerById(playerId), Times.Once);
    mockStorage.Verify(x => x.CreatePlayer(It.IsAny<Player>()), Times.Never);
    Assert.IsNotNull(nextContext);
    Assert.AreEqual(playerId, VerifyPlayerMiddleware.GetValidatedPlayerIdFromContext(nextContext));
  }

  [TestMethod]
  [DataRow("invalid")]
  public async Task ValidUserButInvalidSecret_DoesNotSetContextButCallsNext(string invalidSecret)
  {
    // Arrange
    Guid playerId = Guid.NewGuid();
    string secret = Guid.NewGuid().ToString();
    string salt = Guid.NewGuid().ToString();
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = playerId.ToString();
    httpContext.Request.Headers["X-OpenSkull-UserSecret"] = invalidSecret;

    var mockStorage = new Mock<IPlayerStorage>();
    mockStorage.Setup(x => x.GetPlayerById(playerId))
      .ReturnsAsync(new Player {
        Id = playerId,
        HashedSecret = VerifyPlayerMiddleware.HashSecret(secret, salt),
        Salt = salt
      });

    var mockDelegate = new Mock<RequestDelegate>();
    HttpContext? nextContext = null;
    mockDelegate.Setup(x => x(It.IsAny<HttpContext>()))
      .Callback<HttpContext>((con) => nextContext = con);

    var middleware = new VerifyPlayerMiddleware(mockDelegate.Object, mockStorage.Object, null);
    // Act
    await middleware.InvokeAsync(httpContext);

    // Assert
    mockDelegate.Verify(x => x(httpContext), Times.Once);
    mockStorage.Verify(x => x.GetPlayerById(playerId), Times.Once);
    mockStorage.Verify(x => x.CreatePlayer(It.IsAny<Player>()), Times.Never);
    Assert.IsNotNull(nextContext);
    Assert.IsNull(VerifyPlayerMiddleware.GetValidatedPlayerIdFromContext(nextContext));
  }

  [TestMethod]
  public async Task ValidUserButNoSecret_DoesNotSetContextButCallsNext()
  {
    // Arrange
    Guid playerId = Guid.NewGuid();
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = playerId.ToString();

    var mockStorage = new Mock<IPlayerStorage>();

    var mockDelegate = new Mock<RequestDelegate>();
    HttpContext? nextContext = null;
    mockDelegate.Setup(x => x(It.IsAny<HttpContext>()))
      .Callback<HttpContext>((con) => nextContext = con);

    var middleware = new VerifyPlayerMiddleware(mockDelegate.Object, mockStorage.Object, null);
    // Act
    await middleware.InvokeAsync(httpContext);

    // Assert
    mockDelegate.Verify(x => x(httpContext), Times.Once);
    mockStorage.Verify(x => x.GetPlayerById(It.IsAny<Guid>()), Times.Never);
    mockStorage.Verify(x => x.CreatePlayer(It.IsAny<Player>()), Times.Never);
    Assert.IsNotNull(nextContext);
    Assert.IsNull(VerifyPlayerMiddleware.GetValidatedPlayerIdFromContext(nextContext));
  }

  [TestMethod]
  public async Task NoHeaders_JustCallsContext()
  {
    // Arrange
    Guid playerId = Guid.NewGuid();
    var httpContext = new DefaultHttpContext();

    var mockStorage = new Mock<IPlayerStorage>();

    var mockDelegate = new Mock<RequestDelegate>();
    HttpContext? nextContext = null;
    mockDelegate.Setup(x => x(It.IsAny<HttpContext>()))
      .Callback<HttpContext>((con) => nextContext = con);

    var middleware = new VerifyPlayerMiddleware(mockDelegate.Object, mockStorage.Object, null);
    // Act
    await middleware.InvokeAsync(httpContext);

    // Assert
    mockDelegate.Verify(x => x(httpContext), Times.Once);
    mockStorage.Verify(x => x.GetPlayerById(It.IsAny<Guid>()), Times.Never);
    mockStorage.Verify(x => x.CreatePlayer(It.IsAny<Player>()), Times.Never);
    Assert.IsNotNull(nextContext);
    Assert.IsNull(VerifyPlayerMiddleware.GetValidatedPlayerIdFromContext(nextContext));
  }

  [TestMethod]
  public async Task HappyPath_PlayerDoesntExistYet_CreatesPlayer()
  {
    // Arrange
    Guid playerId = Guid.NewGuid();
    string secret = Guid.NewGuid().ToString();
    string salt = Guid.NewGuid().ToString();
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Headers["X-OpenSkull-UserId"] = playerId.ToString();
    httpContext.Request.Headers["X-OpenSkull-UserSecret"] = secret;

    var mockStorage = new Mock<IPlayerStorage>();
    mockStorage.Setup(x => x.GetPlayerById(playerId))
      .ReturnsAsync(StorageError.NotFound);

    Player? storedPlayer = null;
    mockStorage.Setup(x => x.CreatePlayer(It.IsAny<Player>()))
      .Callback<Player>(player => storedPlayer = player)
      .ReturnsAsync(storedPlayer!);

    var mockDelegate = new Mock<RequestDelegate>();
    HttpContext? nextContext = null;
    mockDelegate.Setup(x => x(It.IsAny<HttpContext>()))
      .Callback<HttpContext>((con) => nextContext = con);

    var mockSaltGenerator = new Mock<VerificationSaltGenerator>();
    mockSaltGenerator.Setup(x => x()).Returns(salt);

    var middleware = new VerifyPlayerMiddleware(mockDelegate.Object, mockStorage.Object, mockSaltGenerator.Object);
    // Act
    await middleware.InvokeAsync(httpContext);

    // Assert
    mockDelegate.Verify(x => x(httpContext), Times.Once);
    mockStorage.Verify(x => x.GetPlayerById(playerId), Times.Once);
    mockStorage.Verify(x => x.CreatePlayer(It.IsAny<Player>()), Times.Once);
    mockSaltGenerator.Verify(x => x(), Times.Once);

    Assert.IsNotNull(storedPlayer);
    Assert.AreEqual(playerId, storedPlayer.Id);
    Assert.IsTrue(VerifyPlayerMiddleware.HashSecret(secret, salt).SequenceEqual(storedPlayer.HashedSecret));
    Assert.AreEqual(salt, storedPlayer.Salt);

    Assert.IsNotNull(nextContext);
    Assert.AreEqual(playerId, VerifyPlayerMiddleware.GetValidatedPlayerIdFromContext(nextContext));
  }
}