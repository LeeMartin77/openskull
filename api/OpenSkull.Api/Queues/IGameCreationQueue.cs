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
  Task FindPlayerInQueues(Guid playerId);
  Task LeaveQueues(Guid playerId);
  Task JoinGameQueue(Guid playerId, int gameSize);
  Task GameMasterThread();
}
