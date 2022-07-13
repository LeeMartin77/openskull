using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;
using OpenSkull.Api.Queue;

namespace OpenSkull.Api.Hubs;

public class PlayerHub : Hub
{
    private readonly IGameCreationQueue _gameCreationQueue;
    
    public PlayerHub(
        IGameCreationQueue gameCreationQueue
    )
    {
        _gameCreationQueue = gameCreationQueue;
    }

    public async Task SubscribeToUserId(string userId) {
        if(userId != null) {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }

    public async Task GetQueueStatus(string playerId)
    {
        Guid parsedPlayerId;
        if (!Guid.TryParse(playerId, out parsedPlayerId)) {
            throw new InvalidOperationException();
        }
        await _gameCreationQueue.FindPlayerInQueues(parsedPlayerId);
    }

    public async Task LeaveQueues(string playerId)
    {
        Guid parsedPlayerId;
        if (!Guid.TryParse(playerId, out parsedPlayerId)) {
            throw new InvalidOperationException();
        }
        await _gameCreationQueue.LeaveQueues(parsedPlayerId);
    }

    public async Task JoinQueue(string playerId, int gameSize)
    {
        Guid parsedPlayerId;
        if (!Guid.TryParse(playerId, out parsedPlayerId)) {
            throw new InvalidOperationException();
        }
        await _gameCreationQueue.JoinGameQueue(parsedPlayerId, gameSize);
    }

}