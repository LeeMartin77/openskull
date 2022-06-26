using OpenSkull.Api.DTO;

namespace OpenSkull.Api.Storage;

public record struct GameStorage {
  public Guid GameId;
  public string VersionTag;
  public Game Game;
}

public enum StorageError {
  CantStore,
  NotFound,
  VersionMismatch
}

public interface IGameStorage {
  Task<Result<GameStorage, StorageError>> StoreNewGame(Game game);
  Task<Result<GameStorage, StorageError>> GetGameById(Guid gameId);
  Task<Result<GameStorage, StorageError>> UpdateGame(GameStorage gameStorage);
}

public class GameMemoryStorage : IGameStorage {
  private List<GameStorage> _gameStorage = new List<GameStorage>();
  public Task<Result<GameStorage, StorageError>> StoreNewGame(Game game)
  {
    var newGameStorage = new GameStorage {
      GameId = Guid.NewGuid(),
      VersionTag = Guid.NewGuid().ToString(),
      Game = game
    };
    _gameStorage.Add(newGameStorage);
    return Task.FromResult<Result<GameStorage, StorageError>>(newGameStorage);
  }

  public Task<Result<GameStorage, StorageError>> GetGameById(Guid gameId)
  {
    var gameIndex = _gameStorage.FindIndex(x => x.GameId == gameId);
    if (gameIndex == -1) {
      return Task.FromResult<Result<GameStorage, StorageError>>(StorageError.NotFound);
    }
    return Task.FromResult<Result<GameStorage, StorageError>>(_gameStorage[gameIndex]);
  }

  public Task<Result<GameStorage, StorageError>> UpdateGame(GameStorage gameStorage)
  {
    var gameIndex = _gameStorage.FindIndex(x => x.GameId == gameStorage.GameId);
    if (gameIndex == -1) {
      return Task.FromResult<Result<GameStorage, StorageError>>(StorageError.NotFound);
    }
    if (_gameStorage[gameIndex].VersionTag != gameStorage.VersionTag) {
      return Task.FromResult<Result<GameStorage, StorageError>>(StorageError.VersionMismatch);
    }
    _gameStorage[gameIndex] = _gameStorage[gameIndex] with {
      VersionTag = Guid.NewGuid().ToString(),
      Game = gameStorage.Game
    };
    return Task.FromResult<Result<GameStorage, StorageError>>(_gameStorage[gameIndex]);
  }

}