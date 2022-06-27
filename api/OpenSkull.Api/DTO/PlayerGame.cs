namespace OpenSkull.Api.DTO;

public record class PlayerGame : PublicGame, IGameView {
  public Guid PlayerId;
  public int PlayerIndex;
  public Card[] PlayerCards;
  public Guid[][] PlayerRoundCardIdsPlayed;
  public PlayerGame(Guid playerId, Game game) : base(game) {
    int playerIndex = Array.IndexOf(game.PlayerIds, playerId);
    PlayerId = playerId;
    PlayerIndex = playerIndex;
    PlayerCards = game.PlayerCards[playerIndex];
    PlayerRoundCardIdsPlayed = game.RoundPlayerCardIds.Select(x => x[playerIndex].ToArray()).ToArray();
  }
}