using System.Net.WebSockets;
using System.Text;

namespace OpenSkull.Api.Messaging;

public enum WebSocketAdditionError {

}

public enum WebSocketType {
  Game,
  Player
}

public record struct OpenskullMessage 
{
  public Guid Id { get; set; }
  public string Activity { get; set; }
}

public interface IWebsocketManager {
  Task<Result<bool, WebSocketAdditionError>> AddWebSocketConnection(WebSocketType type, Guid id, WebSocket websocket);
  Task BroadcastToConnectedWebsockets(WebSocketType type, Guid id, OpenskullMessage message);
}

// This in-memory manager is essentially for demo/testing
// We will need a distributed message bus at some point
public class InMemoryWebsocketManager : IWebsocketManager {
  private readonly List<(Guid, List<WebSocket>)>[] _managedWebsockets;
  public InMemoryWebsocketManager() {
    var websocketsToManage = Enum.GetNames(typeof(WebSocketType)).Length;
    _managedWebsockets = new List<(Guid, List<WebSocket>)>[websocketsToManage];
    for (int i = 0; i < websocketsToManage; i++) {
      _managedWebsockets[i] = new List<(Guid, List<WebSocket>)>();
    }
  }

  public Task<Result<bool, WebSocketAdditionError>> AddWebSocketConnection(WebSocketType type, Guid id, WebSocket websocket)
  {
    var index = _managedWebsockets[(int)type].FindIndex(x => x.Item1 == id);
    if (index > -1) {
      _managedWebsockets[(int)type][index].Item2.Add(websocket);
    } else {
      _managedWebsockets[(int)type].Add((id, new List<WebSocket>() { websocket }));
    }
    return Task.FromResult<Result<bool, WebSocketAdditionError>>(true);
  }

  public async Task BroadcastToConnectedWebsockets(WebSocketType type, Guid id, OpenskullMessage message)
  {
    var index = _managedWebsockets[(int)type].FindIndex(x => x.Item1 == id);
    if (index > -1) {
      byte[] bytes = Encoding.ASCII.GetBytes(System.Text.Json.JsonSerializer.Serialize(message));
      var canToken = new CancellationToken();
      var messageTasks = _managedWebsockets[(int)type][index].Item2.Where(x => x.State == WebSocketState.Open)
        .Select(x => x.SendAsync(bytes, WebSocketMessageType.Text, true, canToken));
      try {
        await Task.WhenAll(messageTasks);
      } catch (Exception ex) {
        // We're basically drowning this
        Console.WriteLine(ex.Message);
      }
    }
  }
}