using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Messaging;
using OpenSkull.Api.Middleware;
using OpenSkull.Api.Queue;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Hubs;

public class PlayerHub : Hub
{
    private readonly IGameCreationQueue _gameCreationQueue;
    private readonly IPlayerStorage _playerStorage;
    private readonly IRoomStorage _roomStorage;
    private readonly IWebSocketManager _websocketManager;
    private readonly GameCreateNew _gameCreateNew;
    private readonly IGameStorage _gameStorage;
    
    public PlayerHub(
        IGameCreationQueue gameCreationQueue,
        IPlayerStorage playerStorage,
        IRoomStorage roomStorage,
        IWebSocketManager webSocketManager,
        IGameStorage gameStorage,
        GameCreateNew gcn
    )
    {
        _gameCreationQueue = gameCreationQueue;
        _playerStorage = playerStorage;
        _roomStorage = roomStorage;
        _websocketManager = webSocketManager;
        _gameStorage = gameStorage;
        _gameCreateNew = gcn;
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

    public async Task UpdateNickname(string userId, string userSecret, string newNickname) {
        Guid playerId;

        if(userId != null && 
            Guid.TryParse(userId, out playerId) && 
            (await VerifyPlayerMiddleware.ValidatePlayerId(_playerStorage, VerifyPlayerMiddleware.DefaultSaltGenerator, playerId, userSecret)) != null) {
            var player = await _playerStorage.GetPlayerById(playerId);
            if (player.IsSuccess)
            {
                var playerDetails = player.Value;
                await _playerStorage.UpdatePlayer(playerDetails with { Nickname = newNickname });
            }
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

    private static void ValidateRoomId(string roomId) {
        var check = Regex.Match(roomId, "^[a-zA-Z0-9_-]*");
        if (!check.Success || check.Captures.Count != 1 || check.Length != roomId.Length) {
            throw new InvalidOperationException("Invalid Room Id");
        }
    }

    public async Task JoinRoom(string playerId, string userSecret, string roomId)
    {
        ValidateRoomId(roomId);
        Guid parsedPlayerId;
        if (!Guid.TryParse(playerId, out parsedPlayerId) ||
            (await VerifyPlayerMiddleware.ValidatePlayerId(_playerStorage, VerifyPlayerMiddleware.DefaultSaltGenerator, parsedPlayerId, userSecret)) == null) {
            throw new InvalidOperationException();
        }
        await _roomStorage.AddPlayerIdToRoom(roomId, parsedPlayerId);
        Context.Items["RoomPlayer"] = playerId;
        await RoomUpdate(roomId);
    }

    public async Task CreateRoomGame(string playerId, string userSecret, string roomId)
    {
        ValidateRoomId(roomId);
        Guid parsedPlayerId;
        if (!Guid.TryParse(playerId, out parsedPlayerId) ||
            (await VerifyPlayerMiddleware.ValidatePlayerId(_playerStorage, VerifyPlayerMiddleware.DefaultSaltGenerator, parsedPlayerId, userSecret)) == null) {
            throw new InvalidOperationException();
        }
        var playerIds = await _roomStorage.GetRoomPlayerIds(roomId);
        if (!playerIds.Contains(parsedPlayerId))
        {
            throw new InvalidOperationException();
        }
        var result = _gameCreateNew(playerIds.ToArray());
        if (result.IsFailure) {
            throw new InvalidOperationException();
        }
        var storageResult = await _gameStorage.StoreNewGame(result.Value);
        if (storageResult.IsFailure) {
            throw new InvalidOperationException();
        }
        await Task.WhenAll(storageResult.Value.Game.PlayerIds
          .Select(id => 
              _websocketManager.BroadcastToConnectedWebsockets(WebSocketType.Player, id, new OpenskullMessage { 
                Id = storageResult.Value.Id, 
                Activity = "GameCreated" 
              })
          )
        );
    }

    public async Task LeaveRoom(string playerId, string userSecret, string roomId)
    {
        ValidateRoomId(roomId);
        Guid parsedPlayerId;
        if (!Guid.TryParse(playerId, out parsedPlayerId) ||
            (await VerifyPlayerMiddleware.ValidatePlayerId(_playerStorage, VerifyPlayerMiddleware.DefaultSaltGenerator, parsedPlayerId, userSecret)) == null) {
            throw new InvalidOperationException();
        }
        await _roomStorage.RemovePlayerIdFromRoom(roomId, parsedPlayerId);
        await RoomUpdate(roomId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        object? playerId = null;
        if(Context.Items.TryGetValue("RoomPlayer", out playerId))
        {
            Guid parsedId = Guid.Parse((string)playerId!);
            var removedRooms = await _roomStorage.RemovePlayerIdFromAllRooms(parsedId);
            await Task.WhenAll(removedRooms.Select(RoomUpdate));
        }
        // TODO: Should throw players out of queues too
        await base.OnDisconnectedAsync(exception);
    }

    private async Task RoomUpdate(string roomId)
    {
        List<Guid> roomPlayers = await _roomStorage.GetRoomPlayerIds(roomId);
        var players = await _playerStorage.GetPlayersByIds(roomPlayers.ToArray());
        if (players.IsSuccess)
        {
            string[] playerNicknames = players.Value.Select(x => x.Nickname).ToArray();
            await Task.WhenAll(roomPlayers.Select(x => _websocketManager.BroadcastToConnectedWebsockets(
                WebSocketType.Player, 
                x, 
                new OpenskullMessage { 
                    Activity = "RoomUpdate", 
                    RoomDetails = new RoomStatus 
                    { 
                        RoomId = roomId, 
                        PlayerNicknames = playerNicknames 
                    } 
                }
            )));
        }
    }

}