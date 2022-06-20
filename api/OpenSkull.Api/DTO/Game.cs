namespace OpenSkull.Api.DTO;


public enum CardType {
  Flower,
  Skull
}

public enum CardState {
  InHand
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
}