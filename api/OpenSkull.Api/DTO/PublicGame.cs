namespace OpenSkull.Api.DTO;

public record class PublicGame {
  public int ActivePlayerIndex;
  public int PlayerCount;
  public int PlayerCardStartingCount;
  public int RoundNumber;
  public int[] CurrentCountPlayerCardsAvailable;
  public int[] CurrentBids;
  public int[][] RoundCountPlayerCardsPlayed;
  public CardType[][][] RoundPlayerCardsRevealed;  
  public int[] RoundWinners;
  public bool GameComplete;

  public PublicGame(Game game) {
    ActivePlayerIndex = game.ActivePlayerIndex;
    PlayerCount = game.PlayerIds.Length;
    PlayerCardStartingCount = game.PlayerCards[0].Length;
    RoundNumber = game.RoundPlayerCardIds.Count();
    CurrentCountPlayerCardsAvailable = game.PlayerCards.Select(x => x.Count(y => y.State != CardState.Discarded)).ToArray();
    RoundCountPlayerCardsPlayed = game.RoundPlayerCardIds.Select(x => x.Select(y => y.Count()).ToArray()).ToArray();
    CurrentBids = game.RoundBids.Last().ToArray();
    RoundPlayerCardsRevealed = game.RoundPlayerCardIds.Select(x => x.Select((y, playerIndex) => y.Select(z => game.PlayerCards[playerIndex].First(c => c.Id == z).Type).ToArray()).ToArray()).ToArray();
    RoundWinners = game.RoundWinPlayerIndexes.ToArray();
    GameComplete = game.GameComplete;
  }
}