using OpenSkull.Api.Storage;

namespace OpenSkull.Api.DTO;

public record class PlayerGame : PublicGame, IGameView {
  public Guid PlayerId { get; set; }
  public int PlayerIndex { get; set; }
  public Card[] PlayerCards { get; set; }
  public Guid[][] PlayerRoundCardIdsPlayed { get; set; }
  public PlayerGame(Guid gameId, Guid playerId, Game game, Player[]? players, DateTime lastUpdated) : base(gameId, game, players, lastUpdated) {
    int playerIndex = Array.IndexOf(game.PlayerIds, playerId);
    PlayerId = playerId;
    PlayerIndex = playerIndex;
    PlayerCards = game.PlayerCards[playerIndex];
    PlayerRoundCardIdsPlayed = game.RoundPlayerCardIds.Select(x => x[playerIndex].ToArray()).ToArray();
  }
}