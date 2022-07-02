using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace OpenSkull.Api.Hubs;

public class GameHub : Hub
{   public override Task OnConnectedAsync()
    {
        var httpCtx = Context.GetHttpContext();
        if (httpCtx != null) {
            StringValues gameId;
            if (httpCtx.Request.Headers.TryGetValue("X-OpenSkull-GameId", out gameId)) {
                Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
            }
        }
        return base.OnConnectedAsync();
    }
}