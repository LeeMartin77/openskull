using OpenSkull.Api.DTO;

namespace OpenSkull.Api.Storage;

public class GameMemoryStorage : IGameStorage {
  private Dictionary<Guid, GameStorage> _gameStorage = new Dictionary<Guid, GameStorage>();

  public Task<Result<GameStorage, StorageError>> StoreNewGame(Game game)
  {
    var newGameStorage = new GameStorage {
      Id = Guid.NewGuid(),
      VersionTag = Guid.NewGuid().ToString(),
      Game = game
    };
    _gameStorage.Add(newGameStorage.Id, newGameStorage);
    return Task.FromResult<Result<GameStorage, StorageError>>(newGameStorage);
  }

  public Task<Result<GameStorage, StorageError>> GetGameById(Guid gameId)
  {
    GameStorage game;
    if (!_gameStorage.TryGetValue(gameId, out game)) {
      return Task.FromResult<Result<GameStorage, StorageError>>(StorageError.NotFound);
    }
    return Task.FromResult<Result<GameStorage, StorageError>>(game);
  }


  public Task<Result<GameStorage[], StorageError>> SearchGames(GameSearchParameters parameters) 
  {
    var playerGames = _gameStorage.Values.Where(x => x.Game.PlayerIds.Contains(parameters.PlayerId));
    return Task.FromResult<Result<GameStorage[], StorageError>>(playerGames.ToArray());
  }

  public Task<Result<GameStorage, StorageError>> UpdateGame(GameStorage gameStorage)
  {
    GameStorage game;
    if (!_gameStorage.TryGetValue(gameStorage.Id, out game)) {
      return Task.FromResult<Result<GameStorage, StorageError>>(StorageError.NotFound);
    }
    if (game.VersionTag != gameStorage.VersionTag) {
      return Task.FromResult<Result<GameStorage, StorageError>>(StorageError.VersionMismatch);
    }
    _gameStorage[gameStorage.Id] = game with {
      VersionTag = Guid.NewGuid().ToString(),
      Game = gameStorage.Game
    };
    return Task.FromResult<Result<GameStorage, StorageError>>(_gameStorage[gameStorage.Id]);
  }

}