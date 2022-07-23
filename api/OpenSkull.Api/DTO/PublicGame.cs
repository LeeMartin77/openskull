using OpenSkull.Api.Storage;

namespace OpenSkull.Api.DTO;

public enum RoundPhase {
  PlayFirstCards,
  PlayCards,
  Bidding,
  Flipping
}

public interface IGameView {
  Guid Id { get; set; }
  int ActivePlayerIndex { get; set; }
  int PlayerCount { get; set; }
  string[] PlayerNicknames { get; set; }
  int PlayerCardStartingCount { get; set; }
  int RoundNumber { get; set; }
  int[] CurrentCountPlayerCardsAvailable { get; set; }
  int[] CurrentBids { get; set; }
  RoundPhase CurrentRoundPhase { get; set; }
  int[][] RoundCountPlayerCardsPlayed { get; set; }
  CardType[][][] RoundPlayerCardsRevealed { get; set; }
  int[] RoundWinners { get; set; }
  bool GameComplete { get; set; }
}

public record class PublicGame : IGameView {
  public Guid Id { get; set; }
  public int ActivePlayerIndex { get; set; }
  public int PlayerCount { get; set; }
  public string[] PlayerNicknames { get; set; }
  public int PlayerCardStartingCount { get; set; }
  public int RoundNumber { get; set; }
  public int[] CurrentCountPlayerCardsAvailable { get; set; }
  public int[] CurrentBids { get; set; }
  public RoundPhase CurrentRoundPhase { get; set; }
  public int[][] RoundCountPlayerCardsPlayed { get; set; }
  public CardType[][][] RoundPlayerCardsRevealed { get; set; }
  public int[] RoundWinners { get; set; }
  public bool GameComplete { get; set; }

  public PublicGame(Guid gameId, Game game, Player[]? players) {
    Id = gameId;
    ActivePlayerIndex = game.ActivePlayerIndex;
    PlayerCount = game.PlayerIds.Length;
    if (players != null) {
      PlayerNicknames = game.PlayerIds.Select((id, i) => {
        int pindex = Array.FindIndex(players, 0, players.Length, p => p.Id == id);
        if (pindex == -1) {
          return $"Player {i}";
        }
        return players[pindex].Nickname;
      }).ToArray();
    } else {
      PlayerNicknames = new string[0];
    }
    PlayerCardStartingCount = game.PlayerCards[0].Length;
    RoundNumber = game.RoundPlayerCardIds.Count();
    CurrentCountPlayerCardsAvailable = game.PlayerCards.Select(x => x.Count(y => y.State != CardState.Discarded)).ToArray();
    RoundCountPlayerCardsPlayed = game.RoundPlayerCardIds.Select(x => x.Select(y => y.Count()).ToArray()).ToArray();
    CurrentBids = game.RoundBids.Last().ToArray();
    // This is HORRIFYING but at least it has a test
    RoundPlayerCardsRevealed = game.RoundPlayerCardIds.Select((x, roundIndex) => x.Select((y, playerIndex) => 
      y.Where((z, cardIndex) => cardIndex < game.RoundRevealedCardPlayerIndexes[roundIndex].Count(j => j == playerIndex)).Select(z => game.PlayerCards[playerIndex].First(c => c.Id == z).Type).ToArray()).ToArray()
    ).ToArray();
    RoundWinners = game.RoundWinPlayerIndexes.ToArray();
    GameComplete = game.GameComplete;
    // Test?
    if (CurrentCountPlayerCardsAvailable.Count(x => x > 0) > RoundCountPlayerCardsPlayed.Last().Count(x => x > 0)) 
    {
      CurrentRoundPhase = RoundPhase.PlayFirstCards;
    } else if (CurrentBids.Count(x => x == 0) == CurrentCountPlayerCardsAvailable.Count(x => x > 0)) {
      CurrentRoundPhase = RoundPhase.PlayCards;
    } else if (CurrentBids.Count(x => x == -1) < CurrentCountPlayerCardsAvailable.Count(x => x > 0) - 1) {
      CurrentRoundPhase = RoundPhase.Bidding;
    } else {
      CurrentRoundPhase = RoundPhase.Flipping;
    }
  }
}