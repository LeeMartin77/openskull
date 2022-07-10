using OpenSkull.Api.DTO;

namespace OpenSkull.Api.Storage;

public interface IGameStorage {
  Task<Result<GameStorage, StorageError>> StoreNewGame(Game game);
  Task<Result<GameStorage, StorageError>> GetGameById(Guid gameId);
  Task<Result<GameStorage[], StorageError>> SearchGames(GameSearchParameters parameters);
  Task<Result<GameStorage, StorageError>> UpdateGame(GameStorage gameStorage);
}

public record struct GameStorage {
  public Guid Id;
  public string VersionTag;
  public Game Game;
}

public record struct GameSearchParameters {
  public Guid[] PlayerIds;
  public bool? GameComplete;
}

public enum StorageError {
  CantStore,
  NotFound,
  VersionMismatch
}

public static class StorageErrorMessages {
  public static string[] StringValues = new string[] {
    "Can't Store Game (System Error)",
    "Game Not Found",
    "Version Mismatch (Item already updated)"
  };
}