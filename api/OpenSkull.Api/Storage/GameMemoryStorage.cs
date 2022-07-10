using OpenSkull.Api.DTO;

namespace OpenSkull.Api.Storage;

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