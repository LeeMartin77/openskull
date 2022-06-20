namespace OpenSkull.Api.DTO;

public record struct Game {
  public Guid Id;
  public Guid[] PlayerIds;
}