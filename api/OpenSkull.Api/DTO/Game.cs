namespace OpenSkull.Api.DTO;


public enum CardType {
  Flower,
  Skull
}

public enum CardState {
  InPlay,
  Discarded
}

public record struct Card {
  public Guid Id { get; set; }
  public CardType Type { get; set; }
  public CardState State { get; set; }
}

public record struct Game {
  public Guid[] PlayerIds { get; set; }
  public Card[][] PlayerCards { get; set; }
  public int ActivePlayerIndex { get; set; }
  public List<List<Guid>[]> RoundPlayerCardIds { get; set; }
  public List<int[]> RoundBids { get; set; }
  public List<List<int>> RoundRevealedCardPlayerIndexes { get; set; }
  public List<int> RoundWinPlayerIndexes { get; set; }
  public bool GameComplete { get; set; }
}