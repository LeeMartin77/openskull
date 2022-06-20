using OpenSkull.Api.DTO;

namespace OpenSkull.Api.Functions;

public enum GameCreationError {
  InvalidNumberOfPlayers
}

public static class GameFunctions {
  private static int MAX_PLAYERS = 6;
  private static int MIN_PLAYERS = 3;
  public static Result<Game, GameCreationError> CreateNew(Guid[] playerIds) {
    if (playerIds == null || playerIds.Length > MAX_PLAYERS || playerIds.Length < MIN_PLAYERS) {
      return Result.Failure<Game, GameCreationError>(GameCreationError.InvalidNumberOfPlayers);
    }
    return Result.Success<Game, GameCreationError>(new Game {
      Id = new Guid(),
      PlayerIds = playerIds
    });
  }
}