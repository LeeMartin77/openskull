using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using OpenSkull.Api.DTO;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Controllers;

public interface IGameTurnInputs {
    string Action { get; set; }
}

public record class PlayCardTurnInputs : IGameTurnInputs
{
  public string Action { get; set; } = "Card";
  public Guid CardId { get; set; }
}

public record class PlaceBidTurnInputs : IGameTurnInputs
{
  public string Action { get; set; } = "Bid";
  public int Bid { get; set; }
}

public record class FlipCardTurnInputs : IGameTurnInputs
{
  public string Action { get; set; } = "Flip";
  public int TargetPlayerIndex { get; set; }
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

    public static string[] ActionStrings = new string[3] { "Card", "Bid", "Flip" };

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
    public async Task<ActionResult<IGameView>> PlayGameTurn(Guid gameId, [FromBody] IGameTurnInputs? inputs = null) {
        
        StringValues rawPlayerId;
        Guid playerId;
        if (Request == null || Request.Headers == null ||
            !Request.Headers.TryGetValue("X-OpenSkull-UserId", out rawPlayerId) ||
            !Guid.TryParse(rawPlayerId.ToString(), out playerId) ||
            inputs == null ||
            !ActionStrings.Contains(inputs.Action)
            ) {
            return new BadRequestResult();
        }

        // TODO: ValidateFieldsOnActions
        
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
        if (inputs.Action == GameController.ActionStrings[0]) {
            postActionResult = _turnPlayCard(game, playerId, (inputs as PlayCardTurnInputs)!.CardId);
        } else if (inputs.Action == GameController.ActionStrings[1]) {
            postActionResult = _turnPlaceBid(game, playerId, (inputs as PlaceBidTurnInputs)!.Bid);
        } else {
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