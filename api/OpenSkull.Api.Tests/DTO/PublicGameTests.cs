using OpenSkull.Api.DTO;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Tests.Functions;

[TestClass]
public class DTO_PublicGame_Tests
{
  [TestMethod]
  [DataRow(3)]
  [DataRow(4)]
  [DataRow(5)]
  [DataRow(6)]
  // Absolute rats nest of a test, but easier than back and forth
  // on the frontend
  public void HappyPath_StateIsInSync(int playerCount)
  {
    Guid[] TestPlayerIds = new Guid[playerCount];
    for (int j = 0; j < playerCount; j++) {
      TestPlayerIds[j] = Guid.NewGuid();
    }
    var game = GameFunctions.CreateNew(TestPlayerIds).Value;

    for (int r = 0; r < 2; r++) {
      var publicGame = new PublicGame(Guid.NewGuid(), game, new Player[0]);
      Assert.AreEqual(RoundPhase.PlayFirstCards, publicGame.CurrentRoundPhase);
      for (int i = 0; i < playerCount; i++) {
        game = GameFunctions.TurnPlayCard(game, game.PlayerIds[i], game.PlayerCards[i][r].Id).Value;
        publicGame = new PublicGame(Guid.NewGuid(), game, new Player[0]);
        if (i < playerCount - 1) {
          Assert.AreEqual(RoundPhase.PlayFirstCards, publicGame.CurrentRoundPhase);
        } else {
          Assert.AreEqual(RoundPhase.PlayCards, publicGame.CurrentRoundPhase);
        }
      }
      
      game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[0], playerCount).Value;
      publicGame = new PublicGame(Guid.NewGuid(), game, new Player[0]);
      Assert.AreEqual(RoundPhase.Bidding, publicGame.CurrentRoundPhase);
      for (int i = 1; i < playerCount; i++) {
        game = GameFunctions.TurnPlaceBid(game, game.PlayerIds[i], GameFunctions.SKIP_BIDDING_VALUE).Value;
        publicGame = new PublicGame(Guid.NewGuid(), game, new Player[0]);
        if (i < playerCount - 1) {
          Assert.AreEqual(RoundPhase.Bidding, publicGame.CurrentRoundPhase);
        } else {
          Assert.AreEqual(RoundPhase.Flipping, publicGame.CurrentRoundPhase);
        }
      }
      publicGame = new PublicGame(Guid.NewGuid(), game, new Player[0]);
      Assert.IsTrue(publicGame.RoundPlayerCardsRevealed.Last().All(x => x.Length == 0));
      for (int i = 0; i < playerCount; i++) {
        game = GameFunctions.TurnFlipCard(game, game.PlayerIds[0], i).Value;
        publicGame = new PublicGame(Guid.NewGuid(), game, new Player[0]);
        if (r == 0 && i == playerCount - 1) {
          Assert.AreEqual(RoundPhase.PlayFirstCards, publicGame.CurrentRoundPhase);
        } else {          
          Assert.AreEqual(i + 1, publicGame.RoundPlayerCardsRevealed.Last().Count(x => x.Length > 0));
          Assert.AreEqual(RoundPhase.Flipping, publicGame.CurrentRoundPhase);
        }
      }
    }
  }
}