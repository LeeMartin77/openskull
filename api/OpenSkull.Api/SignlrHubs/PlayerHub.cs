using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace OpenSkull.Api.Hubs;

public class PlayerHub : Hub
{
    public async Task SubscribeToUserId(string userId) {
        if(userId != null) {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}