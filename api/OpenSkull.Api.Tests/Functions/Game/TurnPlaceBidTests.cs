using OpenSkull.Api.DTO;
using OpenSkull.Api.Functions;

namespace OpenSkull.Api.Tests.Functions;

[TestClass]
public class GameFunction_TurnPlaceBid_Tests
{
  [TestMethod]
  [DataRow(3, 1)]
  [DataRow(3, 2)]
  [DataRow(3, 3)]
  [DataRow(4, 1)]
  [DataRow(4, 2)]
  [DataRow(4, 3)]
  [DataRow(4, 4)]
  [DataRow(5, 1)]
  [DataRow(5, 2)]
  [DataRow(5, 3)]
  [DataRow(5, 4)]
  [DataRow(5, 5)]
  [DataRow(6, 1)]
  [DataRow(6, 2)]
  [DataRow(6, 3)]
  [DataRow(6, 4)]
  [DataRow(6, 5)]
  [DataRow(6, 6)]
  public void HappyPath_GivenAllPlayersHavePlayedCard_CanPlaceBid(int playerCount, int playerBid)
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
    var resultOfPlaceBid = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], playerBid);
    Assert.IsTrue(resultOfPlaceBid.IsSuccess);
    var placeBidGame = resultOfPlaceBid.Value;
    Assert.AreEqual(playerBid, placeBidGame.RoundBids[0][0]);
  }
  
  [TestMethod]
  [DataRow(3, 4)]
  [DataRow(4, 5)]
  [DataRow(5, 6)]
  [DataRow(6, 7)]
  public void GivenBiddingAvailable_CannotBidHigherThanCountOfCards(int playerCount, int playerBid)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var gameResult = GameFunctions.CreateNew(TestPlayerIds);
    var game = gameResult.Value;
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][0].Id).Value;
    }
    var resultOfPlaceBid = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], playerBid);
    Assert.IsTrue(resultOfPlaceBid.IsFailure);
    Assert.AreEqual(GameTurnError.MaxBidExceeded, resultOfPlaceBid.Error);
  }

  [TestMethod]
  [DataRow(3, 2)]
  [DataRow(4, 3)]
  [DataRow(5, 3)]
  [DataRow(6, 4)]
  public void HappyPath_GivenBiddingStarted_CanSkipBidding(int playerCount, int startingBid)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][0].Id).Value;
    }
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], startingBid).Value;
    var gameResult = GameFunctions.TurnPlaceBid(game, game.PlayerIds[1], GameFunctions.SKIP_BIDDING_VALUE);
    Assert.IsTrue(gameResult.IsSuccess);
    Assert.AreEqual(GameFunctions.SKIP_BIDDING_VALUE, gameResult.Value.RoundBids[0][1]);
  }

  [TestMethod]
  public void HappyPath_GivenBidding_SkippedBidPlayersArentActive()
  {
    int playerCount = 4;
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
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[1], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[2], 2).Value;
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[3], 3).Value;
    var gameResult = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], 4);
    Assert.IsTrue(gameResult.IsSuccess);
    Assert.AreEqual(2, gameResult.Value.ActivePlayerIndex);
    Assert.IsTrue(GameFunctions.TurnPlaceBid(gameResult.Value, gameResult.Value.PlayerIds[2], 5).IsSuccess);
  }

  [TestMethod]
  public void GivenBidding_AllOtherPlayersSkipped_CantBid()
  {
    int playerCount = 4;
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
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], 3).Value;
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[1], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[2], 4).Value;
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[3], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], GameFunctions.SKIP_BIDDING_VALUE).Value;
    Assert.AreEqual(game.ActivePlayerIndex, 2);
    var gameResult = GameFunctions.TurnPlaceBid(game, game.PlayerIds[2], 5);
    Assert.IsTrue(gameResult.IsFailure);
    Assert.AreEqual(gameResult.Error, GameTurnError.BiddingHasFinished);
  }

  [TestMethod]
  [DataRow(3, 2)]
  [DataRow(4, 3)]
  [DataRow(5, 3)]
  [DataRow(6, 4)]
  public void GivenBiddingStarted_MustBidHigherThanCurrentBid(int playerCount, int startingBid)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var gameResult = GameFunctions.CreateNew(TestPlayerIds);
    var game = gameResult.Value;
    for (int i = 0; i < playerCount; i++) {
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][0].Id).Value;
    }
    game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], startingBid).Value;
    Assert.AreEqual(GameTurnError.MinBidNotMet, GameFunctions.TurnPlaceBid(game, game.PlayerIds[1], startingBid).Error);
    Assert.AreEqual(GameTurnError.MinBidNotMet, GameFunctions.TurnPlaceBid(game, game.PlayerIds[1], startingBid - 1).Error);
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
    for (int i = 0; i < playerCount; i++) {
      var expectedCardGuid = game.PlayerCards[i][0].Id;
      var postTurnGameResult = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], expectedCardGuid);
      game = postTurnGameResult.Value;
      var actualCardGuid = game.RoundPlayerCardIds[0][i][0];
      Assert.AreEqual(expectedCardGuid, actualCardGuid);
    }
    for (int i = 1; i < playerCount; i++) {
      var expectedCardGuid = game.PlayerCards[i][0].Id;
      var error = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], 1).Error;
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
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    
    var postTurnGameResult = GameFunctions.TurnPlayCard(game, Guid.NewGuid(), game.PlayerCards[0][0].Id);
    Assert.IsTrue(postTurnGameResult.IsFailure);
    postTurnGameResult.OnFailure(err => Assert.AreEqual(GameTurnError.InvalidPlayerId, err));
    
    for (int i = 0; i < playerCount; i++) {
      var expectedCardGuid = game.PlayerCards[i][0].Id;
      game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], expectedCardGuid).Value;
      var actualCardGuid = game.RoundPlayerCardIds[0][i][0];
      Assert.AreEqual(expectedCardGuid, actualCardGuid);
    }
    var error = GameFunctions.TurnPlaceBid(game, Guid.NewGuid(), 1).Error;
    Assert.AreEqual(GameTurnError.InvalidPlayerId, error);
  }

  [TestMethod]
  [DataRow(3, 0)]
  [DataRow(3, 1)]
  [DataRow(3, 2)]
  [DataRow(4, 0)]
  [DataRow(4, 1)]
  [DataRow(4, 2)]
  [DataRow(4, 3)]
  [DataRow(5, 0)]
  [DataRow(5, 1)]
  [DataRow(5, 2)]
  [DataRow(5, 3)]
  [DataRow(5, 4)]
  [DataRow(6, 0)]
  [DataRow(6, 1)]
  [DataRow(6, 2)]
  [DataRow(6, 3)]
  [DataRow(6, 4)]
  [DataRow(6, 5)]
  public void GivenNotAllPlayersHavePlayedCard_CannotPlaceBid(int playerCount, int cardsToPlayBeforeBid)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var gameResult = GameFunctions.CreateNew(TestPlayerIds);
    var game = gameResult.Value;
    for (int i = 0; i < cardsToPlayBeforeBid; i++) {
      var expectedCardGuid = game.PlayerCards[i][0].Id;
      var postTurnGameResult = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], expectedCardGuid);
      game = postTurnGameResult.Value;
      var actualCardGuid = game.RoundPlayerCardIds[0][i][0];
      Assert.AreEqual(expectedCardGuid, actualCardGuid);
    }
    var resultOfPlaceBid = GameFunctions.TurnPlaceBid(game, game.PlayerIds[cardsToPlayBeforeBid], 1);
    Assert.IsTrue(resultOfPlaceBid.IsFailure);
    Assert.AreEqual(GameTurnError.CannotBidYet, resultOfPlaceBid.Error);
  }
}
