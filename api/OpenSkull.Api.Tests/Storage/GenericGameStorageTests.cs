// The idea here is that storage should be interchangeable
// So write a suite of tests, then when we add storage types,
// They have to pass them

using OpenSkull.Api.DTO;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Tests;

[TestClass]
public class GenericGameStorageTests {
  [TestMethod]
  public async Task CanStoreGame() {
    // Arrange
    var storage = new GameMemoryStorage();
    var game = new Game();

    // Act
    var result = await storage.StoreNewGame(game);

    // Assert
    Assert.IsTrue(result.IsSuccess);
  }

  [TestMethod]
  public async Task CanStoreAndRetreiveGame() {
    // Arrange
    var storage = new GameMemoryStorage();
    // Setting a mad value for identification
    var game = new Game {
      ActivePlayerIndex = 999
    };

    // Act
    var stored = (await storage.StoreNewGame(game)).Value;
    var retreived = await storage.GetGameById(stored.GameId);

    // Assert
    Assert.IsTrue(retreived.IsSuccess);
    Assert.AreEqual(stored.GameId, retreived.Value.GameId);
    Assert.AreEqual(stored.Game, retreived.Value.Game);
  }

  [TestMethod]
  public async Task RetreivingUnknownIdReturnsError() {
    // Arrange
    var storage = new GameMemoryStorage();
    // Setting a mad value for identification
    var game = new Game {
      ActivePlayerIndex = 999
    };

    // Act
    var stored = (await storage.StoreNewGame(game)).Value;
    var retreived = await storage.GetGameById(Guid.NewGuid());

    // Assert
    Assert.IsTrue(retreived.IsFailure);
    Assert.AreEqual(StorageError.NotFound, retreived.Error);
  }

  [TestMethod]
  public async Task CanStoreAndUpdateGame() {
    // Arrange
    var storage = new GameMemoryStorage();
    // Setting a mad value for identification
    var game = new Game {
      ActivePlayerIndex = 999
    };

    // Act
    var stored = (await storage.StoreNewGame(game)).Value;
    var updateInput = stored with { Game = stored.Game with { ActivePlayerIndex = 9999 } };
    var updated = await storage.UpdateGame(updateInput);

    // Assert
    Assert.IsTrue(updated.IsSuccess);
    Assert.AreEqual(stored.GameId, updated.Value.GameId);
    Assert.AreNotEqual(stored.VersionTag, updated.Value.VersionTag);
    Assert.AreNotEqual(stored.Game, updated.Value.Game);
    Assert.AreEqual(updateInput.Game, updated.Value.Game);
  }

  [TestMethod]
  public async Task TryToUpdateGameNotStored_ReturnsError() {
    // Arrange
    var storage = new GameMemoryStorage();
    // Setting a mad value for identification
    var game = new Game {
      ActivePlayerIndex = 999
    };

    // Act
    var stored = (await storage.StoreNewGame(game)).Value;
    var updateInput = stored with { Game = stored.Game with { ActivePlayerIndex = 9999 }, GameId = Guid.NewGuid() };
    var updated = await storage.UpdateGame(updateInput);
    var storedAfterUpdate = (await storage.GetGameById(stored.GameId)).Value;


    // Assert
    Assert.IsTrue(updated.IsFailure);
    Assert.AreEqual(StorageError.NotFound, updated.Error);
    Assert.AreEqual(stored, storedAfterUpdate);
  }

  [TestMethod]
  public async Task UpdateWithOldVersionTagReturnsErrorAndDoesNotUpdate() {
        // Arrange
    var storage = new GameMemoryStorage();
    // Setting a mad value for identification
    var game = new Game {
      ActivePlayerIndex = 999
    };

    // Act
    var stored = (await storage.StoreNewGame(game)).Value;
    var updateInput = stored with { 
      Game = stored.Game with { ActivePlayerIndex = 9999 },
      VersionTag = Guid.NewGuid().ToString() 
      };
    var updated = await storage.UpdateGame(updateInput);
    var storedAfterUpdate = (await storage.GetGameById(stored.GameId)).Value;


    // Assert
    Assert.IsTrue(updated.IsFailure);
    Assert.AreEqual(StorageError.VersionMismatch, updated.Error);
    Assert.AreEqual(stored, storedAfterUpdate);
  }
}