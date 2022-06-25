using OpenSkull.Api.DTO;
using OpenSkull.Api.Functions;

namespace OpenSkull.Api.Tests.Functions;

[TestClass]
public class GameFunction_TurnFlipCard_Tests
{ 
  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void HappyPath_EndOfBidding_CanFlipFirstCard(int playerCount) {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][0].Id).Value;
    }
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][1].Id).Value;
    }
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], i + 1).Value;
    }
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], playerCount + 1).Value;
    for (int i = 1; i < playerCount; i++) {
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], GameFunctions.SKIP_BIDDING_VALUE).Value;
    }
    var gameResult = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], 0);
    Assert.IsTrue(gameResult.IsSuccess);
    Assert.AreEqual(1, gameResult.Value.RoundRevealedCardPlayerIndexes.First().Count());
    Assert.AreEqual(0, gameResult.Value.RoundRevealedCardPlayerIndexes.First().First());
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void HappyPath_AllCardsToBetFlipped_NoSkull_StartNewRound(int playerCount) {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][0].Id).Value;
    }
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], playerCount).Value;
    for (int i = 1; i < playerCount; i++) {
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], GameFunctions.SKIP_BIDDING_VALUE).Value;
    }
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], i).Value;
    }
    Assert.AreEqual(0, game.ActivePlayerIndex);
    Assert.AreEqual(2, game.RoundBids.Count());
    Assert.AreEqual(2, game.RoundPlayerCardIds.Count());
    Assert.AreEqual(2, game.RoundRevealedCardPlayerIndexes.Count());
    Assert.AreEqual(1, game.RoundWinPlayerIndexes.Count());
    Assert.AreEqual(0, game.RoundWinPlayerIndexes[0]);
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void HappyPath_SkullFlipped_StartNewRound(int playerCount) {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    for (int i = 0; i < playerCount - 1; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][0].Id).Value;
    }
    // Bit hardcodey on this skull ID, but it should be fine - revisit at some point
    int skullplayer = playerCount - 1;
    game = GameFunctions.TurnPlayCard(game, game.PlayerIds[skullplayer], game.PlayerCards[skullplayer][3].Id).Value;

    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], playerCount).Value;
    for (int i = 1; i < playerCount; i++) {
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], GameFunctions.SKIP_BIDDING_VALUE).Value;
    }
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], i).Value;
    }
    Assert.AreEqual(playerCount - 1, game.ActivePlayerIndex);
    Assert.AreEqual(2, game.RoundBids.Count());
    Assert.AreEqual(2, game.RoundPlayerCardIds.Count());
    Assert.AreEqual(2, game.RoundRevealedCardPlayerIndexes.Count());
    Assert.AreEqual(0, game.RoundWinPlayerIndexes.Count());
    Assert.AreEqual(3, game.PlayerCards[0].Where(x => x.State != CardState.Discarded).Count());
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void NotActivePlayer_CannotAttemptToFlipCard(int playerCount) {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][0].Id).Value;
    }
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][1].Id).Value;
    }
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], i + 1).Value;
    }
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], playerCount + 1).Value;
    for (int i = 1; i < playerCount; i++) {
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], GameFunctions.SKIP_BIDDING_VALUE).Value;
    }
    for (int i = 1; i < playerCount; i++) {
      var gameResult = GameFunctions.TurnFlipCard(game, game.PlayerIds[i], i);
      Assert.IsTrue(gameResult.IsFailure);
      Assert.AreEqual(GameTurnError.InvalidPlayerId, gameResult.Error);
    }
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void HaveCardsLeftToActivePlayer_CannotFlipOtherPlayersYet(int playerCount) {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][0].Id).Value;
    }
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][1].Id).Value;
    }
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], i + 1).Value;
    }
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], playerCount + 1).Value;
    for (int i = 1; i < playerCount; i++) {
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], GameFunctions.SKIP_BIDDING_VALUE).Value;
    }
    for (int i = 1; i < playerCount; i++) {
      var failResult = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], i);
      Assert.IsTrue(failResult.IsFailure);
      Assert.AreEqual(GameTurnError.MustRevealAllOwnCardsFirst, failResult.Error);
    }
    game = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], 0).Value;
    for (int i = 1; i < playerCount; i++) {
      var failResult = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], i);
      Assert.IsTrue(failResult.IsFailure);
      Assert.AreEqual(GameTurnError.MustRevealAllOwnCardsFirst, failResult.Error);
    }
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void HaveFlippedAllOwnCards_CanFlipOtherPlayersCards(int playerCount) {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][0].Id).Value;
    }
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][1].Id).Value;
    }
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], i + 1).Value;
    }
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], playerCount + 1).Value;
    for (int i = 1; i < playerCount; i++) {
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], GameFunctions.SKIP_BIDDING_VALUE).Value;
    }
    game = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], 0).Value;
    game = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], 0).Value;
    var gameResult = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], 1);
    Assert.IsTrue(gameResult.IsSuccess);
    Assert.AreEqual("0,0,1", string.Join(",", gameResult.Value.RoundRevealedCardPlayerIndexes.Last().Select(x => x.ToString()).ToArray()));
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void NoCardsLeftWithPlayer_Error(int playerCount) {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][0].Id).Value;
    }
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][1].Id).Value;
    }
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], i + 1).Value;
    }
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], playerCount + 1).Value;
    for (int i = 1; i < playerCount; i++) {
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], GameFunctions.SKIP_BIDDING_VALUE).Value;
    }
    game = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], 0).Value;
    game = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], 0).Value;
    var gameResult = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], 0);
    Assert.IsTrue(gameResult.IsFailure);
    Assert.AreEqual(GameTurnError.NoCardsLeftToFlip, gameResult.Error);
  }
}