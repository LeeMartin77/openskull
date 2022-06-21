using OpenSkull.Api.DTO;
using OpenSkull.Api.Functions;

namespace OpenSkull.Api.Tests.Functions;


[TestClass]
public class GameFunctionTests
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
  public void GivenCreateNew_PlayerPointsIsRightLengthAllZero(int playerCount)
  {
      Guid[] TestPlayerIds = new Guid[playerCount];
      for (int i = 0; i < playerCount; i++) {
        TestPlayerIds[i] = Guid.NewGuid();
      }
      var GameResult = GameFunctions.CreateNew(TestPlayerIds);
      Assert.IsTrue(GameResult.IsSuccess);
      GameResult.Tap((game) => {
        Assert.AreEqual(playerCount, game.PlayerPoints.Length);
        Assert.IsTrue(game.PlayerPoints.All(x => x == 0));
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
      Assert.AreEqual(1, game.RoundPlayerCards.Count());
      Assert.AreEqual(playerCount, game.RoundPlayerCards.First().Length);
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
}