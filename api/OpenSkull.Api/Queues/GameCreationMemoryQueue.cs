using OpenSkull.Api.Functions;
using OpenSkull.Api.Messaging;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Queue;

public class GameCreationMemoryQueue : IGameCreationQueue
{
  private Queue<Guid>[] _gameCreationQueue;
  private readonly GameCreateNew _gameCreateNew;
  private readonly IGameStorage _gameStorage;
  private readonly IWebSocketManager _webSocketManager;

  public GameCreationMemoryQueue(GameCreateNew gameCreationFunction, IGameStorage gameStorage, IWebSocketManager webSocketManager) {
    int queuesToCreate = GameFunctions.MAX_PLAYERS - GameFunctions.MIN_PLAYERS + 1;
    _gameCreationQueue = new Queue<Guid>[queuesToCreate];
    for (int i = 0; i < queuesToCreate; i++) {
      _gameCreationQueue[i] = new Queue<Guid>();
    }
    _gameStorage = gameStorage;
    _gameCreateNew = gameCreationFunction;
    _webSocketManager = webSocketManager;
  }

  public Task<Result<PlayerQueueStatus, PlayerQueueStatusError>> FindPlayerInQueues(Guid playerId) {
    int queueIndex = Array.FindIndex(_gameCreationQueue, x => x.Contains(playerId));
    if(queueIndex == -1) {
      return Task.FromResult<Result<PlayerQueueStatus, PlayerQueueStatusError>>(PlayerQueueStatusError.PlayerNotInAnyQueues);
    }

    var status = new PlayerQueueStatus {
      GameSize = queueIndex + GameFunctions.MIN_PLAYERS,
      QueueSize = _gameCreationQueue[queueIndex].Count
    };

    return Task.FromResult<Result<PlayerQueueStatus, PlayerQueueStatusError>>(status);
  }

  public Task<Result<bool, PlayerQueueLeaveError>> LeaveQueues(Guid playerId) {
    int queueIndex = Array.FindIndex(_gameCreationQueue, x => x.Contains(playerId));
    if(queueIndex == -1) {
      return Task.FromResult<Result<bool, PlayerQueueLeaveError>>(PlayerQueueLeaveError.PlayerNotInAnyQueues);
    }
    _gameCreationQueue[queueIndex] = new Queue<Guid>(_gameCreationQueue[queueIndex].Where(x => x != playerId));
    return Task.FromResult<Result<bool, PlayerQueueLeaveError>>(true);
  }


  public Task<Result<int, QueueError>> PlayersInQueue(int gameSize)
  {
    return Task.FromResult<Result<int, QueueError>>(_gameCreationQueue[gameSize - GameFunctions.MIN_PLAYERS].Count);
  }

  public async Task<Result<bool, QueueJoinError>> JoinGameQueue(Guid playerId, int gameSize)
  {
    int queueIndex = gameSize - GameFunctions.MIN_PLAYERS;
    if (queueIndex < 0 || queueIndex > _gameCreationQueue.Length - 1) {
      return QueueJoinError.OutsideGameSize;
    }
    if (_gameCreationQueue.Any(x => x.Contains(playerId))) {
      return QueueJoinError.AlreadyInQueue;
    }
    if (_gameCreationQueue[queueIndex].Count >= gameSize - 1) {
      
      var queuedPlayerIds = new List<Guid>();
      while(queuedPlayerIds.Count < gameSize -1) {
        queuedPlayerIds.Add(_gameCreationQueue[queueIndex].Dequeue());
      }
      var playerIds = queuedPlayerIds.ToList();
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
      try {
        await Task.WhenAll(storedGame.Value.Game.PlayerIds
          .Select(id => 
              _webSocketManager.BroadcastToConnectedWebsockets(WebSocketType.Player, id, new OpenskullMessage { Id = storedGame.Value.Id, Activity = "GameCreated" })
          )
        );
      } catch {
          //Drowning any weird exceptions
      }
      return true;
    }
    _gameCreationQueue[queueIndex].Enqueue(playerId);
    return false;
  }

  public Task GameMasterThread() {
    // Not implemented for memory queue, which creates immediately.
    // As it has no concept of multi-system
    return Task.CompletedTask;
  }
}