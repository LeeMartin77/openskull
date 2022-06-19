using OpenSkull.Api.DTO;

namespace OpenSkull.Api.Functions;

public enum GameCreationError {

}

public static class GameFunctions {
  public static Result<Game, GameCreationError> CreateNew(Guid[] playerIds) {
    return Result.Success<Game, GameCreationError>(new Game());
  }
}