using OpenSkull.Api.DTO;
using OpenSkull.Api.Functions;

namespace OpenSkull.Api.Tests;

[TestClass]
public class Integration_FullGame {
  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  public void FirstPlayerWinsTwice_Complete(int playerCount) {
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
  public void ArbitaryPlayerWinsTwice_Complete(int playerCount, int winner) {
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

  [TestMethod]
  public void PlayerWins_Loses_Wins_GameEnds() {
    int playerCount = 3;
    Guid[] TestPlayerIds = new Guid[3];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[0], game.PlayerCards[0][0].Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[1], game.PlayerCards[1][0].Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[2], game.PlayerCards[2][0].Id).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[0], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[2], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnFlipCard(game, TestPlayerIds[0], 0).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[0], game.PlayerCards[0].First(x => x.Type == CardType.Skull).Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[1], game.PlayerCards[1][0].Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[2], game.PlayerCards[2][0].Id).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[0], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[2], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnFlipCard(game, TestPlayerIds[0], 0).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[0], game.PlayerCards[0].First(x => x.Type != CardType.Skull && x.State != CardState.Discarded).Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[1], game.PlayerCards[1][0].Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[2], game.PlayerCards[2][0].Id).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[0], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[2], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnFlipCard(game, TestPlayerIds[0], 0).Value;
    Assert.IsTrue(game.GameComplete);
    Assert.AreEqual(1, game.PlayerCards[0].Count(x => x.State == CardState.Discarded));
  }

  [TestMethod]
  public void OnePlayerHasLostAllCards_OtherPlayersCanCompleteGame() {
    int playerCount = 3;
    Guid[] TestPlayerIds = new Guid[3];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    // We need to "engineer" this as it'd take a mix of bad luck and bad play
    // By having player one only have one card and it be a skull
    // This is essentially worst-case as it'd mean active player is cardless
    game.PlayerCards[0] = game.PlayerCards[0].Select(x => {
      if (x.Type != CardType.Skull) {
        x.State = CardState.Discarded;
      }
      return x;
    }).ToArray();
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[0], game.PlayerCards[0].First(x => x.Type == CardType.Skull).Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[1], game.PlayerCards[1][0].Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[2], game.PlayerCards[2][0].Id).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[0], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[2], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnFlipCard(game, TestPlayerIds[0], 0).Value;
    // "First" Player has now lost all cards - can player 1 now win alone
    // We will just tick up on player losing all cards
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[1], game.PlayerCards[1][0].Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[2], game.PlayerCards[2][0].Id).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[2], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnFlipCard(game, TestPlayerIds[1], 1).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[1], game.PlayerCards[1][0].Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[2], game.PlayerCards[2][0].Id).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[2], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnFlipCard(game, TestPlayerIds[1], 1).Value;
    Assert.IsTrue(game.GameComplete);
  }

  [TestMethod]
  public void TwoPlayersHaveLostAllCards_OtherPlayersCanCompleteGame() {
    int playerCount = 4;
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    // We need to "engineer" this as it'd take a mix of bad luck and bad play
    // By having player one only have one card and it be a skull
    // This is essentially worst-case as it'd mean active player is cardless
    game.PlayerCards[0] = game.PlayerCards[0].Select(x => {
      if (x.Type != CardType.Skull) {
        x.State = CardState.Discarded;
      }
      return x;
    }).ToArray();
    game.PlayerCards[2] = game.PlayerCards[2].Select(x => {
      if (x.Type != CardType.Skull) {
        x.State = CardState.Discarded;
      }
      return x;
    }).ToArray();
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[0], game.PlayerCards[0].First(x => x.Type == CardType.Skull).Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[1], game.PlayerCards[1][0].Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[2], game.PlayerCards[2].First(x => x.Type == CardType.Skull).Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[3], game.PlayerCards[3][0].Id).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[0], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[2], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[3], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnFlipCard(game, TestPlayerIds[0], 0).Value;

    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[1], game.PlayerCards[1][0].Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[2], game.PlayerCards[2].First(x => x.Type == CardType.Skull).Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[3], game.PlayerCards[3][0].Id).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[2], 2).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[3], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnFlipCard(game, TestPlayerIds[2], 2).Value;

    // Players one and two have now lost all cards
    // We will just tick up on player losing all cards
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[3], game.PlayerCards[3][0].Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[1], game.PlayerCards[1][0].Id).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[3], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnFlipCard(game, TestPlayerIds[3], 3).Value;

    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[3], game.PlayerCards[3][0].Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[1], game.PlayerCards[1][0].Id).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[3], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnFlipCard(game, TestPlayerIds[3], 3).Value;
    Assert.IsTrue(game.GameComplete);
  }

  [TestMethod]
  public void OnlyOnePlayerLeft_GameIsComplete() {
    int playerCount = 3;
    Guid[] TestPlayerIds = new Guid[3];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;
    // We need to "engineer" this as it'd take a mix of bad luck and bad play
    // By having player one only have one card and it be a skull
    // This is essentially worst-case as it'd mean active player is cardless
    game.PlayerCards[0] = game.PlayerCards[0].Select(x => {
      if (x.Type != CardType.Skull) {
        x.State = CardState.Discarded;
      }
      return x;
    }).ToArray();
    game.PlayerCards[1] = game.PlayerCards[1].Select(x => {
      if (x.Type != CardType.Skull) {
        x.State = CardState.Discarded;
      }
      return x;
    }).ToArray();
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[0], game.PlayerCards[0].First(x => x.Type == CardType.Skull).Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[1], game.PlayerCards[1].First(x => x.Type == CardType.Skull).Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[2], game.PlayerCards[2][0].Id).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[0], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[2], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnFlipCard(game, TestPlayerIds[0], 0).Value;
    // "First" Player has now lost all cards - can player 1 now win alone
    // We will just tick up on player losing all cards
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[1], game.PlayerCards[1].First(x => x.Type == CardType.Skull).Id).Value;
    game = GameFunctions.TurnPlayCard(game, TestPlayerIds[2], game.PlayerCards[2][0].Id).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[1], 1).Value;
    game = GameFunctions.TurnPlaceBid(game, TestPlayerIds[2], GameFunctions.SKIP_BIDDING_VALUE).Value;
    game = GameFunctions.TurnFlipCard(game, TestPlayerIds[1], 1).Value;
    
    Assert.IsTrue(game.GameComplete);

    var extraTurn = GameFunctions.TurnFlipCard(game, TestPlayerIds[1], 2);
    Assert.IsTrue(extraTurn.IsFailure);
    
    var extraTurn2 = GameFunctions.TurnFlipCard(game, TestPlayerIds[2], 2);
    Assert.IsTrue(extraTurn2.IsFailure);
  }
}