namespace OpenSkull.Api.DTO;

public interface IGameView {
  Guid Id { get; set; }
  int ActivePlayerIndex { get; set; }
  int PlayerCount { get; set; }
  int PlayerCardStartingCount { get; set; }
  int RoundNumber { get; set; }
  int[] CurrentCountPlayerCardsAvailable { get; set; }
  int[] CurrentBids { get; set; }
  int[][] RoundCountPlayerCardsPlayed { get; set; }
  CardType[][][] RoundPlayerCardsRevealed { get; set; }
  int[] RoundWinners { get; set; }
  bool GameComplete { get; set; }
}

public record class PublicGame : IGameView {
  public Guid Id { get; set; }
  public int ActivePlayerIndex { get; set; }
  public int PlayerCount { get; set; }
  public int PlayerCardStartingCount { get; set; }
  public int RoundNumber { get; set; }
  public int[] CurrentCountPlayerCardsAvailable { get; set; }
  public int[] CurrentBids { get; set; }
  public int[][] RoundCountPlayerCardsPlayed { get; set; }
  public CardType[][][] RoundPlayerCardsRevealed { get; set; }
  public int[] RoundWinners { get; set; }
  public bool GameComplete { get; set; }

  public PublicGame(Guid gameId, Game game) {
    Id = gameId;
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