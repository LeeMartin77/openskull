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
    var retreived = await storage.GetGameById(stored.Id);

    // Assert
    Assert.IsTrue(retreived.IsSuccess);
    Assert.AreEqual(stored.Id, retreived.Value.Id);
    Assert.AreEqual(stored.Game, retreived.Value.Game);
  }


  [TestMethod]
  [DataRow("cb660fda-7ed8-4cd5-9d63-db84f2da732a")]
  public async Task CanStoreAndSearchForGames(string testPlayerId) {
    // Arrange
    var storage = new GameMemoryStorage();
    Guid testPlayerGuid = Guid.Parse(testPlayerId);
    // Setting a mad value for identification
    var foundGame = new Game {
      ActivePlayerIndex = 999,
      PlayerIds = new Guid[] { testPlayerGuid }
    };

    // Act
    await storage.StoreNewGame(foundGame);
    await storage.StoreNewGame(new Game {
      ActivePlayerIndex = 1,
      PlayerIds = new Guid[] { Guid.NewGuid() }
    });

    var searchResults = await storage.SearchGames(new GameSearchParameters {
      PlayerId = testPlayerGuid,
      PageIndex = 0,
      PageLength = 25
    });

    // Assert
    Assert.IsTrue(searchResults.IsSuccess);
    Assert.AreEqual(foundGame, searchResults.Value[0].Game);
    Assert.AreEqual(1, searchResults.Value.Length);
  }

  [TestMethod]
  public async Task SearchWithNoResults_ReturnsEmptyArray() {
    // Arrange
    var storage = new GameMemoryStorage();
    Guid testPlayerGuid = Guid.Parse("cb660fda-7ed8-4cd5-9d63-db84f2da732a");

    // Act
    var searchResults = await storage.SearchGames(new GameSearchParameters {
      PlayerId = testPlayerGuid
    });

    // Assert
    Assert.IsTrue(searchResults.IsSuccess);
    Assert.AreEqual(0, searchResults.Value.Length);
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
    Assert.AreEqual(stored.Id, updated.Value.Id);
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
    var updateInput = stored with { Game = stored.Game with { ActivePlayerIndex = 9999 }, Id = Guid.NewGuid() };
    var updated = await storage.UpdateGame(updateInput);
    var storedAfterUpdate = (await storage.GetGameById(stored.Id)).Value;


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
    var storedAfterUpdate = (await storage.GetGameById(stored.Id)).Value;


    // Assert
    Assert.IsTrue(updated.IsFailure);
    Assert.AreEqual(StorageError.VersionMismatch, updated.Error);
    Assert.AreEqual(stored, storedAfterUpdate);
  }
}