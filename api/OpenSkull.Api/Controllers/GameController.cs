using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using OpenSkull.Api.DTO;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Controllers;

public interface IGameTurnInputs {
    string Action { get; set; }
}

public record struct PlayCardTurnInputs : IGameTurnInputs
{
  public string Action { get; set; }
  public Guid CardId { get; set; }
}

public record struct PlaceBidTurnInputs : IGameTurnInputs
{
  public string Action { get; set; }
  public int Bid { get; set; }
}

public record struct FlipCardTurnInputs : IGameTurnInputs
{
  public string Action { get; set; }
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
            inputs == null
            ) {
            return new BadRequestResult();
        }
        
        var gameResult = await _gameStorage.GetGameById(gameId);
        if (gameResult.IsFailure && gameResult.Error == StorageError.NotFound) {
            return new NotFoundResult();
        }

        var game = gameResult.Value.Game;

        if (!game.PlayerIds.Contains(playerId)) {
            return new ForbidResult();
        }

        throw new NotImplementedException();
    }
}
