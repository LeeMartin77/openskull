using Microsoft.AspNetCore.Mvc;
using OpenSkull.Api.Messaging;

namespace OpenSkull.Api.Controllers;

[ApiController]
public class WebsocketController : ControllerBase
{
    private readonly IWebSocketManager _websocketManager;

    WebsocketController(IWebSocketManager socketManager) {
        _websocketManager = socketManager;
    }

    [HttpGet("games/{gameId}/ws")]
    public async Task GetGameWebsocketConnection(Guid gameId)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            if (webSocket != null) {
                await _websocketManager.AddWebSocketConnection(WebSocketType.Game, gameId, webSocket);
            }
        } else {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    [HttpGet("player/{playerId}/ws")]
    public async Task GetPlayerWebsocketConnection(Guid playerId)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            if (webSocket != null) {
                await _websocketManager.AddWebSocketConnection(WebSocketType.Player, playerId, webSocket);
            }
        } else {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}