using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using OpenSkull.Api.DTO;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Messaging;
using OpenSkull.Api.Middleware;
using OpenSkull.Api.Queue;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Controllers;

public record class PlayTurnInputs
{
  public string? Action { get; set; }
  public Guid? CardId { get; set; }
  public int? Bid { get; set; }
  public int? TargetPlayerIndex { get; set; }
}

public enum TurnAction {
    Card,
    Bid,
    Flip
}


[ApiController]
public class GameController : ControllerBase
{
    private readonly ILogger<GameController> _logger;
    private readonly IGameStorage _gameStorage;
    private readonly IPlayerStorage _playerStorage;
    private readonly IGameCreationQueue _gameCreationQueue;
    private readonly IWebSocketManager _webSocketManager;
    private readonly GameCreateNew _gameCreateNew;
    private readonly TurnPlayCard _turnPlayCard;
    private readonly TurnPlaceBid _turnPlaceBid;
    private readonly TurnFlipCard _turnFlipCard;

    public GameController(
        ILogger<GameController> logger,
        IGameStorage gameStorage,
        IPlayerStorage playerStorage,
        IGameCreationQueue gameCreationQueue,
        IWebSocketManager webSocketManager,
        GameCreateNew gcn,
        TurnPlayCard tpc,
        TurnPlaceBid tpb,
        TurnFlipCard tfc
    )
    {
        _logger = logger;
        _gameStorage = gameStorage;
        _playerStorage = playerStorage;
        _gameCreationQueue = gameCreationQueue;
        _webSocketManager = webSocketManager;
        _gameCreateNew = gcn;
        _turnPlayCard = tpc;
        _turnPlaceBid = tpb;
        _turnFlipCard = tfc;
    }

    public class TestGameInput {
        public List<Guid>? PlayerIds { get; set; }
    }

    // No tests for this, as this is to enable testing in itself
    [Route("api/games/createtestgame")]
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateTestGame([FromBody] TestGameInput input) {
        var result = _gameCreateNew(input.PlayerIds != null ? input.PlayerIds.ToArray() : new Guid[0]);
        if (result.IsFailure) {
            return BadRequest();
        }
        var storageResult = await _gameStorage.StoreNewGame(result.Value);
        if (storageResult.IsFailure) {
            throw new Exception(storageResult.Error.ToString());
        }
        return storageResult.Value.Id;
    }

    public record struct GameQueueParameters {
        public int GameSize { get; set; }
    }

    [Route("api/games")]
    [HttpGet]
    public async Task<ActionResult<IGameView[]>> SearchGames(
        [FromQuery] bool? gameComplete = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageLength = 25
        )
    {
        Guid? seekPlayerId = VerifyPlayerMiddleware.GetValidatedPlayerIdFromContext(HttpContext);
        if (seekPlayerId == null) {
            return BadRequest("Must have UserId Header");
        }
        Guid playerId = (Guid)seekPlayerId;
        var searchResult = await _gameStorage.SearchGames(new GameSearchParameters { 
            PlayerId = playerId,
            PageIndex = pageIndex,
            PageLength = pageLength
            });
        if (searchResult.IsFailure) {
            // TODO: Right now, this shouldn't ever actually fail...
            throw new NotImplementedException();
        }
        return searchResult.Value.Select(x => {
            if (x.Game.PlayerIds.Contains(playerId)) {
                return new PlayerGame(x.Id, playerId, x.Game, new Player[0], x.LastUpdated);
            }
            return new PublicGame(x.Id, x.Game, new Player[0], x.LastUpdated);
        }).ToArray();
    }
    
    [Route("api/games/{gameId}")]
    [HttpGet]
    public async Task<ActionResult<IGameView>> GetGame(Guid gameId) {
        var gameResult = await _gameStorage.GetGameById(gameId);
        if (gameResult.IsFailure && gameResult.Error == StorageError.NotFound) {
            return new NotFoundResult();
        }
        
        Guid? playerId = VerifyPlayerMiddleware.GetValidatedPlayerIdFromContext(HttpContext);
        Player[] players = new Player[0];
        (await _playerStorage.GetPlayersByIds(gameResult.Value.Game.PlayerIds)).Map(prs => players = prs);
        if (playerId != null &&
            gameResult.Value.Game.PlayerIds.Contains((Guid)playerId)
            ) {
            return new PlayerGame(gameId, (Guid)playerId, gameResult.Value.Game, players, gameResult.Value.LastUpdated);
        }
        return new PublicGame(gameId, gameResult.Value.Game, players, gameResult.Value.LastUpdated);
    }

    [Route("api/games/{gameId}/turn")]
    [HttpPost]
    public async Task<ActionResult<IGameView>> PlayGameTurn(Guid gameId, [FromBody] PlayTurnInputs? inputs = null) {
        Guid? seekPlayerId = VerifyPlayerMiddleware.GetValidatedPlayerIdFromContext(HttpContext);
        TurnAction action;
        if (seekPlayerId == null || inputs == null ||
            !Enum.TryParse(inputs.Action, out action)
            ) {
            return new BadRequestResult();
        }
        Guid playerId = (Guid)seekPlayerId;
        switch(action) {
            case TurnAction.Card:
                if (inputs.CardId is null) {
                    return new BadRequestResult();
                }
                break;
            case TurnAction.Bid:
                if (inputs.Bid is null) {
                    return new BadRequestResult();
                }
                break;
            case TurnAction.Flip:
                if (inputs.TargetPlayerIndex is null) {
                    return new BadRequestResult();
                }
                break;
            default:
                throw new NotImplementedException();
        }

        var gameResult = await _gameStorage.GetGameById(gameId);
        if (gameResult.IsFailure && gameResult.Error == StorageError.NotFound) {
            return new NotFoundResult();
        }

        var game = gameResult.Value.Game;

        if (!game.PlayerIds.Contains(playerId)) {
            return new ForbidResult();
        }

        Result<Game, GameTurnError> postActionResult;
        // Want to use a switch statement here somehow
        // probably want an enum parsing thing
        switch(action) {
            case TurnAction.Card:
                postActionResult = _turnPlayCard(game, playerId, inputs.CardId ?? throw new Exception());
                break;
            case TurnAction.Bid:
                postActionResult = _turnPlaceBid(game, playerId, inputs.Bid ?? throw new Exception());
                break;
            case TurnAction.Flip:
                postActionResult = _turnFlipCard(game, playerId, inputs.TargetPlayerIndex ?? throw new Exception());
                break;
            default:
                throw new NotImplementedException();
        }
        if (postActionResult.IsFailure) {
            return new BadRequestObjectResult(GameFunctions.GameTurnErrorMessage[(int)postActionResult.Error]);
        }
        var storageResult = await _gameStorage.UpdateGame(gameResult.Value with {
            Game = postActionResult.Value
        });
        if (storageResult.IsFailure) {
            switch (storageResult.Error) {
                case StorageError.CantStore:
                    // There is probably a nicer way of doing this
                    // But I can't find it right now
                    var response = new ObjectResult(StorageErrorMessages.StringValues[(int)storageResult.Error]);
                    response.StatusCode = 500;
                    return response;
                case StorageError.VersionMismatch:
                    return new ConflictObjectResult(StorageErrorMessages.StringValues[(int)storageResult.Error]);
                default: 
                    throw new Exception();
            }
        }
        await _webSocketManager.BroadcastToConnectedWebsockets(WebSocketType.Game, gameId, new OpenskullMessage{ Id = gameId, Activity = "Turn" });
        Player[] players = new Player[0];
        (await _playerStorage.GetPlayersByIds(gameResult.Value.Game.PlayerIds)).Map(prs => players = prs);
        return new PlayerGame(storageResult.Value.Id, playerId, storageResult.Value.Game, players, storageResult.Value.LastUpdated);
    }
}
