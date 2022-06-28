using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using OpenSkull.Api.DTO;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Controllers;

public record class PlayTurnInputs
{
  public string? Action;
  public Guid? CardId;
  public int? Bid;
  public int? TargetPlayerIndex;
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
    private readonly GameCreateNew _gameCreateNew;
    private readonly TurnPlayCard _turnPlayCard;
    private readonly TurnPlaceBid _turnPlaceBid;
    private readonly TurnFlipCard _turnFlipCard;

    public GameController(
        ILogger<GameController> logger,
        IGameStorage gameStorage,
        GameCreateNew gcn,
        TurnPlayCard tpc,
        TurnPlaceBid tpb,
        TurnFlipCard tfc
    )
    {
        _logger = logger;
        _gameStorage = gameStorage;
        _gameCreateNew = gcn;
        _turnPlayCard = tpc;
        _turnPlaceBid = tpb;
        _turnFlipCard = tfc;
    }

    [Route("games/{gameId}")]
    [HttpGet]
    public async Task<ActionResult<IGameView>> GetGame(Guid gameId) {
        var gameResult = await _gameStorage.GetGameById(gameId);
        if (gameResult.IsFailure && gameResult.Error == StorageError.NotFound) {
            return new NotFoundResult();
        }
        StringValues rawPlayerId;
        Guid playerId;
        if (Request != null && Request.Headers != null &&
            Request.Headers.TryGetValue("X-OpenSkull-UserId", out rawPlayerId) &&
            Guid.TryParse(rawPlayerId.ToString(), out playerId) &&
            gameResult.Value.Game.PlayerIds.Contains(playerId)
            ) {
            return new PlayerGame(playerId, gameResult.Value.Game);
        }
        return new PublicGame(gameResult.Value.Game);
    }

    [Route("games/{gameId}/turn")]
    [HttpPost]
    public async Task<ActionResult<IGameView>> PlayGameTurn(Guid gameId, [FromBody] PlayTurnInputs? inputs = null) {
        
        StringValues rawPlayerId;
        Guid playerId;
        TurnAction action;
        if (Request == null || Request.Headers == null ||
            !Request.Headers.TryGetValue("X-OpenSkull-UserId", out rawPlayerId) ||
            !Guid.TryParse(rawPlayerId.ToString(), out playerId) ||
            inputs == null ||
            !Enum.TryParse(inputs.Action, out action)
            ) {
            return new BadRequestResult();
        }

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
        return new PlayerGame(playerId, storageResult.Value.Game);
    }
}
