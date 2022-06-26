using Microsoft.AspNetCore.Mvc;
using OpenSkull.Api.DTO;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Controllers;

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
    public async Task<ActionResult<PublicGame>> GetGame(Guid gameId) {
        var gameResult = await _gameStorage.GetGameById(gameId);
        if (gameResult.IsFailure && gameResult.Error == StorageError.NotFound) {
            return new NotFoundResult();
        }
        throw new NotImplementedException();
    }
}
