namespace OpenSkull.Api.DTO;


public enum CardType {
  Flower,
  Skull
}

public enum CardState {
  Hidden,
  Revealed,
  Discarded
}

public record struct Card {
  public Guid Id;
  public CardType Type;
  public CardState State;
}

public record struct Game {
  public Guid Id;
  public Guid[] PlayerIds;
  public Card[][] PlayerCards;
  public int ActivePlayerIndex;
  public int[] PlayerPoints;
  public List<List<Card>[]> RoundPlayerCards;
}