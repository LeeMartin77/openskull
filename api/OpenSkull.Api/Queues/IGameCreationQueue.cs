using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Queue;

public enum QueueJoinError {
  OutsideGameSize,
  CreationError,
  StorageError,
  AlreadyInQueue
}

public enum QueueError {
  QueueError
}

public enum PlayerQueueStatusError {
  PlayerNotInAnyQueues
}

public enum PlayerQueueLeaveError {
  PlayerNotInAnyQueues
}

public record struct PlayerQueueStatus {
  public int GameSize { get; set; }
  public int QueueSize { get; set; }
}

public interface IGameCreationQueue {
  Task<Result<PlayerQueueStatus, PlayerQueueStatusError>> FindPlayerInQueues(Guid playerId);
  Task<Result<bool, PlayerQueueLeaveError>> LeaveQueues(Guid playerId);
  Task<Result<int, QueueError>> PlayersInQueue(int gameSize);
  Task<Result<bool, QueueJoinError>> JoinGameQueue(Guid playerId, int gameSize);
  Task GameMasterThread();
}
