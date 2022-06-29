using OpenSkull.Api.Functions;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Queue;

public enum QueueJoinError {
  OutsideGameSize,
  CreationError,
  StorageError
}

public enum QueueError {
  QueueError
}

public interface IGameCreationQueue {
  Task<Result<int, QueueError>> PlayersInQueue(int gameSize);
  Task<Result<GameStorage?, QueueJoinError>> JoinGameQueue(Guid playerId, int gameSize);
}

public class GameCreationMemoryQueue : IGameCreationQueue
{
  private Queue<Guid>[] _gameCreationQueue;
  private readonly GameCreateNew _gameCreateNew;
  private readonly IGameStorage _gameStorage;

  public GameCreationMemoryQueue(GameCreateNew gameCreationFunction, IGameStorage gameStorage) {
    int queuesToCreate = GameFunctions.MAX_PLAYERS - GameFunctions.MIN_PLAYERS + 1;
    _gameCreationQueue = new Queue<Guid>[queuesToCreate];
    for (int i = 0; i < queuesToCreate; i++) {
      _gameCreationQueue[i] = new Queue<Guid>();
    }
    _gameStorage = gameStorage;
    _gameCreateNew = gameCreationFunction;
  }


  public Task<Result<int, QueueError>> PlayersInQueue(int gameSize)
  {
    return Task.FromResult<Result<int, QueueError>>(_gameCreationQueue[gameSize - GameFunctions.MIN_PLAYERS].Count);
  }

  public async Task<Result<GameStorage?, QueueJoinError>> JoinGameQueue(Guid playerId, int gameSize)
  {
    int queueIndex = gameSize - GameFunctions.MIN_PLAYERS;
    if (_gameCreationQueue[queueIndex].Count >= gameSize - 1) {
      List<Guid> queuedPlayerIds = new List<Guid>();
      for (int i = 0; i < gameSize - 1; i++) {
        queuedPlayerIds.Add(_gameCreationQueue[queueIndex].Dequeue());
      }
      var playerIds = queuedPlayerIds.ToArray().ToList();
      playerIds.Add(playerId);
      var createdGame = _gameCreateNew(playerIds.ToArray());
      if (createdGame.IsFailure) {
        queuedPlayerIds.ForEach(x => _gameCreationQueue[queueIndex].Enqueue(x));
        return QueueJoinError.CreationError;
      }
      var storedGame = await _gameStorage.StoreNewGame(createdGame.Value);
      if (storedGame.IsFailure) {
        queuedPlayerIds.ForEach(x => _gameCreationQueue[queueIndex].Enqueue(x));
        return QueueJoinError.StorageError;
      }
      // TODO - this will need to broadcast creation
      return storedGame.Value;
    }
    _gameCreationQueue[queueIndex].Enqueue(playerId);
    return null;
  }
}