using OpenSkull.Api.DTO;
using OpenSkull.Api.Functions;

namespace OpenSkull.Api.Tests.Functions;

[TestClass]
public class GameFunction_TurnPlayCard_Tests
{
  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void HappyPath_GivenValidFirstTurn_Succeeds(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int i = 0; i < playerCount; i++) {
      TestPlayerIds[i] = Guid.NewGuid();
    }
    var gameResult = GameFunctions.CreateNew(TestPlayerIds);
    gameResult.Tap(game => {
      var postTurnGame = GameFunctions.TurnPlayCard(game, TestPlayerIds[0], game.PlayerCards[0][0].Id).Value;
      Assert.AreEqual(game.PlayerCards[0][0].Id, postTurnGame.RoundPlayerCardIds.First()[0].First());
    });
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void HappyPath_GivenAllPlayersFirstTurns_Succeeds(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var gameResult = GameFunctions.CreateNew(TestPlayerIds);
    var game = gameResult.Value;
    for (int i = 0; i < playerCount; i++) {
      var expectedCardGuid = game.PlayerCards[i][0].Id;
      var postTurnGameResult = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], expectedCardGuid);
      game = postTurnGameResult.Value;
      var actualCardGuid = game.RoundPlayerCardIds[0][i][0];
      Assert.AreEqual(expectedCardGuid, actualCardGuid);
    }
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void HappyPath_GivenAllPlayersPlayAllCards_Succeeds(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var gameResult = GameFunctions.CreateNew(TestPlayerIds);
    var game = gameResult.Value;
    for (int cardIndex = 0; cardIndex < game.PlayerCards[0].Length; cardIndex++) {
      for (int playerIndex = 0; playerIndex < playerCount; playerIndex++) {
        var expectedCardGuid = game.PlayerCards[playerIndex][cardIndex].Id;
        var postTurnGameResult = GameFunctions.TurnPlayCard(game, game.PlayerIds[playerIndex], expectedCardGuid);
        game = postTurnGameResult.Value;
        Assert.AreEqual(expectedCardGuid, game.RoundPlayerCardIds[0][playerIndex][cardIndex]);
      }
    }
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void GivenPlayerTriesToReplayCard_Errors(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var gameResult = GameFunctions.CreateNew(TestPlayerIds);
    var game = gameResult.Value;
    for (int i = 0; i < playerCount; i++) {
      var expectedCardGuid = game.PlayerCards[i][0].Id;
      var postTurnGameResult = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], expectedCardGuid);
      game = postTurnGameResult.Value;
      var actualCardGuid = game.RoundPlayerCardIds[0][i][0];
      Assert.AreEqual(expectedCardGuid, actualCardGuid);
    }
    var err = GameFunctions.TurnPlayCard(game, game.PlayerIds[0], game.PlayerCards[0][0].Id).Error;
    Assert.AreEqual(GameTurnError.InvalidCardId, err);
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void GivenCardNotHidden_Error(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int i = 0; i < playerCount; i++) {
      TestPlayerIds[i] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    game.PlayerCards[0][0].State = CardState.Discarded;
    var err = GameFunctions.TurnPlayCard(game, TestPlayerIds[0], game.PlayerCards[0][0].Id).Error;
    
    Assert.AreEqual(GameTurnError.InvalidCardId, err);
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void GivenInvalidPlayerOnTurn_Errors(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var gameResult = GameFunctions.CreateNew(TestPlayerIds);
    var game = gameResult.Value;
    for (int i = 1; i < playerCount; i++) {
      var expectedCardGuid = game.PlayerCards[i][0].Id;
      var error = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], expectedCardGuid).Error;
      Assert.AreEqual(GameTurnError.InvalidPlayerId, error);
    }
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void PlayerIdNotFound_Errors(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int i = 0; i < playerCount; i++) {
      TestPlayerIds[i] = Guid.NewGuid();
    }
    var gameResult = GameFunctions.CreateNew(TestPlayerIds);
    gameResult.Tap(game => {
      var postTurnGameResult = GameFunctions.TurnPlayCard(game, Guid.NewGuid(), game.PlayerCards[0][0].Id);
      Assert.IsTrue(postTurnGameResult.IsFailure);
      postTurnGameResult.OnFailure(err => Assert.AreEqual(GameTurnError.InvalidPlayerId, err));
    });
  }
  
  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void CardIdNotInCards_Errors(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int i = 0; i < playerCount; i++) {
      TestPlayerIds[i] = Guid.NewGuid();
    }
    var gameResult = GameFunctions.CreateNew(TestPlayerIds);
    gameResult.Tap(game => {
      var postTurnGameResult = GameFunctions.TurnPlayCard(game, TestPlayerIds[0], Guid.NewGuid());
      Assert.IsTrue(postTurnGameResult.IsFailure);
      postTurnGameResult.OnFailure(err => { Assert.AreEqual(GameTurnError.InvalidCardId, err); });
    });
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void PlayerIdDoesNotHaveCardId_Errors(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int i = 0; i < playerCount; i++) {
      TestPlayerIds[i] = Guid.NewGuid();
    }
    var gameResult = GameFunctions.CreateNew(TestPlayerIds);
    gameResult.Tap(game => {
      var postTurnGameResult = GameFunctions.TurnPlayCard(game, TestPlayerIds[0], game.PlayerCards[1][0].Id);
      Assert.IsTrue(postTurnGameResult.IsFailure);
      postTurnGameResult.OnFailure(err => { Assert.AreEqual(GameTurnError.InvalidCardId, err); });
    });
  }

  
  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void GivenPlayCardAfterBid_Error(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var gameResult = GameFunctions.CreateNew(TestPlayerIds);
    var game = gameResult.Value;
    for (int i = 0; i < playerCount; i++) {
      var expectedCardGuid = game.PlayerCards[i][0].Id;
      var postTurnGameResult = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], expectedCardGuid);
      game = postTurnGameResult.Value;
      var actualCardGuid = game.RoundPlayerCardIds[0][i][0];
      Assert.AreEqual(expectedCardGuid, actualCardGuid);
    }
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], 1).Value;
    var err = GameFunctions.TurnPlayCard(game, game.PlayerIds[1], game.PlayerCards[1][1].Id).Error;
    Assert.AreEqual(GameTurnError.CannotPlayCardAfterBid, err);
  }
}

