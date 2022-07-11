using Microsoft.AspNetCore.SignalR;
using OpenSkull.Api.Hubs;

namespace OpenSkull.Api.Messaging;

// This in-memory manager is essentially for demo/testing
// We will need a distributed message bus at some point
public class InMemoryWebSocketManager : IWebSocketManager {
  private readonly IHubContext<PlayerHub> _playerHubContext;
  private readonly IHubContext<GameHub> _gameHubContext;

  private readonly Queue<(WebSocketType, Guid, OpenskullMessage)> _messages = new Queue<(WebSocketType, Guid, OpenskullMessage)>();

  public InMemoryWebSocketManager(IHubContext<PlayerHub> playerHubContext, IHubContext<GameHub> gameHubContext) {
    _playerHubContext = playerHubContext;
    _gameHubContext = gameHubContext;
  }

  public Task BroadcastToConnectedWebsockets(WebSocketType type, Guid id, OpenskullMessage message)
  {
    _messages.Enqueue((type, id, message));
    return Task.CompletedTask;
  }

  public async Task WebsocketMessageSenderThread()
  {
    (WebSocketType, Guid, OpenskullMessage) message;
    while (true) {
      if (_messages.TryDequeue(out message)) {
        switch (message.Item1) {
          case WebSocketType.Game:
            await _gameHubContext.Clients.Group(message.Item2.ToString()).SendCoreAsync("send", new object[]{message.Item3});
            break;
          case WebSocketType.Player:
            await _playerHubContext.Clients.Group(message.Item2.ToString()).SendCoreAsync("send", new object[]{message.Item3});
            break;
          default:
            throw new NotImplementedException();
        }
      }
    }
  }
}