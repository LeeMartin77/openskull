namespace OpenSkull.Api.Messaging;

public enum WebSocketType {
  Game,
  Player
}

public record struct OpenskullMessage 
{
  public Guid Id { get; set; }
  public string Activity { get; set; }
}

public interface IWebSocketManager {
  Task BroadcastToConnectedWebsockets(WebSocketType type, Guid id, OpenskullMessage message);
  Task WebsocketMessageSenderThread();
}