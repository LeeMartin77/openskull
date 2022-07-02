using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Primitives;

namespace OpenSkull.Api.Hubs;

public class PlayerHub : Hub
{
    public override Task OnConnectedAsync()
    {
        var httpCtx = Context.GetHttpContext();
        if (httpCtx != null) {
            StringValues userId;
            if (httpCtx.Request.Headers.TryGetValue("X-OpenSkull-UserId", out userId)) {
                Groups.AddToGroupAsync(Context.ConnectionId, userId.ToString());
            }
        }

        return base.OnConnectedAsync();
    }
}