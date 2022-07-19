using Microsoft.AspNetCore.SignalR;
using OpenSkull.Api.Messaging;
using OpenSkull.Api.Middleware;
using OpenSkull.Api.Queue;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Hubs;

public class PlayerHub : Hub
{
    private readonly IGameCreationQueue _gameCreationQueue;
    private readonly IPlayerStorage _playerStorage;
    
    public PlayerHub(
        IGameCreationQueue gameCreationQueue,
        IPlayerStorage playerStorage
    )
    {
        _gameCreationQueue = gameCreationQueue;
        _playerStorage = playerStorage;
    }

    public async Task SubscribeToUserId(string userId, string userSecret) {
        Guid playerId;
        if(userId != null && 
            Guid.TryParse(userId, out playerId) && 
            (await VerifyPlayerMiddleware.ValidatePlayerId(_playerStorage, VerifyPlayerMiddleware.DefaultSaltGenerator, playerId, userSecret)) != null) {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            await Clients.Caller.SendCoreAsync("send", new object[]{ new OpenskullMessage { Activity = "Subscribed", Id = playerId } });
        }
    }

    public async Task GetQueueStatus(string playerId, string userSecret)
    {
        Guid parsedPlayerId;
        if (!Guid.TryParse(playerId, out parsedPlayerId) ||
            (await VerifyPlayerMiddleware.ValidatePlayerId(_playerStorage, VerifyPlayerMiddleware.DefaultSaltGenerator, parsedPlayerId, userSecret)) == null) {
            throw new InvalidOperationException();
        }
        await _gameCreationQueue.FindPlayerInQueues(parsedPlayerId);
    }

    public async Task LeaveQueues(string playerId, string userSecret)
    {
        Guid parsedPlayerId;
        if (!Guid.TryParse(playerId, out parsedPlayerId) || 
            (await VerifyPlayerMiddleware.ValidatePlayerId(_playerStorage, VerifyPlayerMiddleware.DefaultSaltGenerator, parsedPlayerId, userSecret)) == null) {
            throw new InvalidOperationException();
        }
        await _gameCreationQueue.LeaveQueues(parsedPlayerId);
    }

    public async Task JoinQueue(string playerId, string userSecret, int gameSize)
    {
        Guid parsedPlayerId;
        if (!Guid.TryParse(playerId, out parsedPlayerId) ||
            (await VerifyPlayerMiddleware.ValidatePlayerId(_playerStorage, VerifyPlayerMiddleware.DefaultSaltGenerator, parsedPlayerId, userSecret)) == null) {
            throw new InvalidOperationException();
        }
        await _gameCreationQueue.JoinGameQueue(parsedPlayerId, gameSize);
    }

}