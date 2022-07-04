using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using OpenSkull.Api.Hubs;

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

public interface IWebSocketManager {
  Task BroadcastToConnectedWebsockets(WebSocketType type, Guid id, OpenskullMessage message);
}

// This in-memory manager is essentially for demo/testing
// We will need a distributed message bus at some point
public class InMemoryWebSocketManager : IWebSocketManager {
  private readonly IHubContext<PlayerHub> _playerHubContext;
  private readonly IHubContext<GameHub> _gameHubContext;

  public InMemoryWebSocketManager(IHubContext<PlayerHub> playerHubContext, IHubContext<GameHub> gameHubContext) {
    _playerHubContext = playerHubContext;
    _gameHubContext = gameHubContext;
  }

  public async Task BroadcastToConnectedWebsockets(WebSocketType type, Guid id, OpenskullMessage message)
  {
    switch (type) {
      case WebSocketType.Game:
        await _gameHubContext.Clients.Group(id.ToString()).SendCoreAsync("send", new object[]{message});
        break;
      case WebSocketType.Player:
        await _playerHubContext.Clients.Group(id.ToString()).SendCoreAsync("send", new object[]{message});
        break;
      default:
        throw new NotImplementedException();
    }
  }
}