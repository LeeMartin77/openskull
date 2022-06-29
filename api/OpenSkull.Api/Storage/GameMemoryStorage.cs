using OpenSkull.Api.DTO;

namespace OpenSkull.Api.Storage;

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

public interface IGameStorage {
  Task<Result<GameStorage, StorageError>> StoreNewGame(Game game);
  Task<Result<GameStorage, StorageError>> GetGameById(Guid gameId);
  Task<Result<GameStorage[], StorageError>> SearchGames(GameSearchParameters parameters);
  Task<Result<GameStorage, StorageError>> UpdateGame(GameStorage gameStorage);
}

public class GameMemoryStorage : IGameStorage {
  private List<GameStorage> _gameStorage = new List<GameStorage>();

  public Task<Result<GameStorage, StorageError>> StoreNewGame(Game game)
  {
    var newGameStorage = new GameStorage {
      Id = Guid.NewGuid(),
      VersionTag = Guid.NewGuid().ToString(),
      Game = game
    };
    _gameStorage.Add(newGameStorage);
    return Task.FromResult<Result<GameStorage, StorageError>>(newGameStorage);
  }

  public Task<Result<GameStorage, StorageError>> GetGameById(Guid gameId)
  {
    var gameIndex = _gameStorage.FindIndex(x => x.Id == gameId);
    if (gameIndex == -1) {
      return Task.FromResult<Result<GameStorage, StorageError>>(StorageError.NotFound);
    }
    return Task.FromResult<Result<GameStorage, StorageError>>(_gameStorage[gameIndex]);
  }


  public Task<Result<GameStorage[], StorageError>> SearchGames(GameSearchParameters parameters) 
  {
    var playerGames = _gameStorage.Where(x => parameters.PlayerIds.Any(y => x.Game.PlayerIds.Contains(y)));
    if (parameters.GameComplete != null) {
      playerGames = playerGames.Where(x => x.Game.GameComplete == parameters.GameComplete);
    }
    return Task.FromResult<Result<GameStorage[], StorageError>>(playerGames.ToArray());
  }

  public Task<Result<GameStorage, StorageError>> UpdateGame(GameStorage gameStorage)
  {
    var gameIndex = _gameStorage.FindIndex(x => x.Id == gameStorage.Id);
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