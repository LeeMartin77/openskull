using OpenSkull.Api.Queue;

namespace OpenSkull.Api.Messaging;

public enum WebSocketType {
  Game,
  Player
}

public record struct RoomStatus
{
  public string RoomId { get; set; }
  public string[] PlayerNicknames { get; set; }
}

public record struct OpenskullMessage 
{
  public Guid Id { get; set; }
  public string Activity { get; set; }
  public PlayerQueueStatus Details { get; set; }
  public RoomStatus RoomDetails { get; set; }
}

public interface IWebSocketManager {
  Task BroadcastToConnectedWebsockets(WebSocketType type, Guid id, OpenskullMessage message);
  Task WebsocketMessageSenderThread();
}