namespace OpenSkull.Api.DTO;


public enum CardType {
  Flower,
  Skull
}

public enum CardState {
  Hidden,
  Discarded
}

public record struct Card {
  public Guid Id;
  public CardType Type;
  public CardState State;
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