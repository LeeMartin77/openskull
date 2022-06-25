using OpenSkull.Api.DTO;
using OpenSkull.Api.Functions;

namespace OpenSkull.Api.Tests.Functions;

[TestClass]
public class GameFunction_CreateNew_Tests
{
  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void HappyPath_GivenValidNumberOfPlayers_CreatesGame(int playerCount)
  {
      Guid[] TestPlayerIds = new Guid[playerCount];
      for (int i = 0; i < playerCount; i++) {
        TestPlayerIds[i] = Guid.NewGuid();
      }
      var GameResult = GameFunctions.CreateNew(TestPlayerIds);
      Assert.IsTrue(GameResult.IsSuccess);
  }

  [TestMethod]
  [DataRow(0)]
  [DataRow(1)]
  [DataRow(2)]
  [DataRow(7)]
  [DataRow(8)]
  [DataRow(9)]

  public void GivenCreateNew_ErrorPath_GivenInvalidNumberOfPlayers_ErrorsAppropriately(int playerCount)
  {
      Guid[] TestPlayerIds = new Guid[playerCount];
      for (int i = 0; i < playerCount; i++) {
        TestPlayerIds[i] = Guid.NewGuid();
      }
      var GameResult = GameFunctions.CreateNew(TestPlayerIds);
      Assert.IsTrue(GameResult.IsFailure);
      GameResult.OnFailure((err) => Assert.AreEqual(GameCreationError.InvalidNumberOfPlayers, err));
  }

  
  [TestMethod]

  public void GivenCreateNew_ErrorPath_GivenDuplicatePlayerId_ErrorsAppropriately()
  {
    const int playerCount = 3;
      Guid[] TestPlayerIds = new Guid[playerCount];
      
      TestPlayerIds[0] = Guid.NewGuid();
      TestPlayerIds[1] = Guid.NewGuid();
      TestPlayerIds[2] = TestPlayerIds[1];

      var GameResult = GameFunctions.CreateNew(TestPlayerIds);
      Assert.IsTrue(GameResult.IsFailure);
      GameResult.OnFailure((err) => Assert.AreEqual(GameCreationError.DuplicatePlayer, err));
  }


  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void GivenCreateNew_EachPlayerHasCards(int playerCount)
  {
      Guid[] TestPlayerIds = new Guid[playerCount];
      for (int i = 0; i < playerCount; i++) {
        TestPlayerIds[i] = Guid.NewGuid();
      }
      var GameResult = GameFunctions.CreateNew(TestPlayerIds);
      Assert.IsTrue(GameResult.IsSuccess);
      GameResult.Tap((game) => {
        Assert.AreEqual(playerCount, game.PlayerCards.Length);
        foreach (Card[] playerCards in game.PlayerCards) {
          Assert.AreEqual(1, playerCards.Count(x => x.Type == CardType.Skull));
          Assert.AreEqual(3, playerCards.Count(x => x.Type == CardType.Flower));
          Assert.AreEqual(playerCards.Length, playerCards.Select(x => x.Id).Distinct().Count());
          Assert.AreEqual(playerCards.Length, playerCards.Where(x => x.State == CardState.Hidden).Count());
        }
        Assert.AreEqual(game.PlayerCards.Length * 4, game.PlayerCards.SelectMany(x => x.Select(y => y.Id)).Distinct().Count());
      });
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void GivenCreateNew_ActivePlayerIndexIsZero(int playerCount)
  {
      Guid[] TestPlayerIds = new Guid[playerCount];
      for (int i = 0; i < playerCount; i++) {
        TestPlayerIds[i] = Guid.NewGuid();
      }
      var GameResult = GameFunctions.CreateNew(TestPlayerIds);
      Assert.IsTrue(GameResult.IsSuccess);
      GameResult.Tap((game) => {
        Assert.AreEqual(0, game.ActivePlayerIndex);
      });
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void GivenCreateNew_GameCompleteIsFalse(int playerCount)
  {
      Guid[] TestPlayerIds = new Guid[playerCount];
      for (int i = 0; i < playerCount; i++) {
        TestPlayerIds[i] = Guid.NewGuid();
      }
      var GameResult = GameFunctions.CreateNew(TestPlayerIds);
      Assert.IsTrue(GameResult.IsSuccess);
      GameResult.Tap((game) => {
        Assert.IsFalse(game.GameComplete);
      });
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void GivenCreateNew_HasFirstRoundWithRightSizeArray(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int i = 0; i < playerCount; i++) {
      TestPlayerIds[i] = Guid.NewGuid();
    }
    var GameResult = GameFunctions.CreateNew(TestPlayerIds);
    Assert.IsTrue(GameResult.IsSuccess);
    GameResult.Tap((game) => {
      Assert.AreEqual(1, game.RoundPlayerCardIds.Count());
      Assert.AreEqual(playerCount, game.RoundPlayerCardIds.First().Length);
    });
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void GivenCreateNew_HasFirstBiddingRoundWithRightSizeArray(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int i = 0; i < playerCount; i++) {
      TestPlayerIds[i] = Guid.NewGuid();
    }
    var GameResult = GameFunctions.CreateNew(TestPlayerIds);
    Assert.IsTrue(GameResult.IsSuccess);
    GameResult.Tap((game) => {
      Assert.AreEqual(1, game.RoundBids.Count());
      Assert.AreEqual(playerCount, game.RoundBids.First().Length);
    });
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void GivenCreateNew_HasFirstRevealRound(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int i = 0; i < playerCount; i++) {
      TestPlayerIds[i] = Guid.NewGuid();
    }
    var GameResult = GameFunctions.CreateNew(TestPlayerIds);
    Assert.IsTrue(GameResult.IsSuccess);
    var game = GameResult.Value;
    Assert.AreEqual(1, game.RoundRevealedCardPlayerIndexes.Count());
    Assert.AreEqual(0, game.RoundRevealedCardPlayerIndexes.First().Count());
  }

  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void GivenCreateNew_RoundWinsCreatedAndEmpty(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int i = 0; i < playerCount; i++) {
      TestPlayerIds[i] = Guid.NewGuid();
    }
    var GameResult = GameFunctions.CreateNew(TestPlayerIds);
    Assert.IsTrue(GameResult.IsSuccess);
    var game = GameResult.Value;
    Assert.AreEqual(0, game.RoundWinPlayerIndexes.Count());
  }
}

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

// TODO: Full Game integration to victory

[TestClass]
public class Integration_FullGame {
  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void HappyPath_FirstPlayerWinsTwice_Complete(int playerCount) {
      Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    for (int r = 0; r < 2; r++) {
      for (int i = 0; i < playerCount; i++) {
        game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][r].Id).Value;
      }
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], playerCount).Value;
      for (int i = 1; i < playerCount; i++) {
        game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], GameFunctions.SKIP_BIDDING_VALUE).Value;
      }
      for (int i = 0; i < playerCount; i++) {
        game = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], i).Value;
      }
    }
    Assert.AreEqual(0, game.ActivePlayerIndex);
    Assert.AreEqual(2, game.RoundBids.Count());
    Assert.AreEqual(2, game.RoundPlayerCardIds.Count());
    Assert.AreEqual(2, game.RoundRevealedCardPlayerIndexes.Count());
    Assert.AreEqual(2, game.RoundWinPlayerIndexes.Count());
    Assert.AreEqual(2, game.RoundWinPlayerIndexes.Count(x => x == 0));
    Assert.IsTrue(game.GameComplete);
    var extraResult = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], 0);
    Assert.IsTrue(extraResult.IsFailure);
  }

  [TestMethod]
  [DataRow(3, 1)]
  [DataRow(4, 3)]
  [DataRow(5, 4)]
  [DataRow(6, 3)]
  public void HappyPath_ArbitaryPlayerWinsTwice_Complete(int playerCount, int winner) {
      Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    for (int r = 0; r < 2; r++) {
      int initValue = game.ActivePlayerIndex;
      do {
        game = GameFunctions.TurnPlayCard(game, game.PlayerIds[game.ActivePlayerIndex], game.PlayerCards[game.ActivePlayerIndex][r].Id).Value;
      } while (game.ActivePlayerIndex != initValue);
      bool nowBidding = false;
      int playerId = game.ActivePlayerIndex;
      do {
        if (!nowBidding && playerId != winner) {
          game = GameFunctions.TurnPlayCard(game, game.PlayerIds[playerId], game.PlayerCards[playerId][r + 1].Id).Value;
        } 
        if (nowBidding && playerId != winner) {
          game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[playerId], GameFunctions.SKIP_BIDDING_VALUE).Value;
        }
        if (playerId == winner) {
          game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[playerId], playerCount).Value;
          nowBidding = true;
        }
        playerId += 1;
        if(playerId == playerCount) {
          playerId = 0;
        }
      } while (!nowBidding || game.RoundBids.Last().Count(x => x == GameFunctions.SKIP_BIDDING_VALUE) < playerCount - 1);
      game = GameFunctions.TurnFlipCard(game, game.PlayerIds[winner], winner).Value;
      for (int i = 0; i < playerCount; i++) {
        if (i != winner) {
          game = GameFunctions.TurnFlipCard(game, game.PlayerIds[winner], i).Value;
        } 
      }
    }
    Assert.AreEqual(winner, game.ActivePlayerIndex);
    Assert.AreEqual(2, game.RoundBids.Count());
    Assert.AreEqual(2, game.RoundPlayerCardIds.Count());
    Assert.AreEqual(2, game.RoundRevealedCardPlayerIndexes.Count());
    Assert.AreEqual(2, game.RoundWinPlayerIndexes.Count());
    Assert.AreEqual(2, game.RoundWinPlayerIndexes.Count(x => x == winner));
    Assert.IsTrue(game.GameComplete);
    var extraResult = GameFunctions.TurnFlipCard(game, game.PlayerIds[winner], 0);
    Assert.IsTrue(extraResult.IsFailure);
  }
}
// Include Handling player with no cards left