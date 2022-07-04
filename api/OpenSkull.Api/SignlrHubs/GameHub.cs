using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace OpenSkull.Api.Hubs;

public class GameHub : Hub
{   
    public async Task SubscribeToGameId(string gameId) {
        if(gameId != null) {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        }
    }
}