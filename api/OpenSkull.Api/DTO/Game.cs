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
  public Guid[] PlayerIds;
  public Card[][] PlayerCards;
  public int ActivePlayerIndex;
  public List<List<Guid>[]> RoundPlayerCardIds;
  public List<int[]> RoundBids;
  public List<List<int>> RoundRevealedCardPlayerIndexes;
  public List<int> RoundWinPlayerIndexes;
  public bool GameComplete;
}