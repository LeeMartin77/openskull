using System.Linq;

namespace OpenSkull.Api.Storage;

public class PlayerMemoryStorage : IPlayerStorage
{
  private List<Player> _memoryPlayers = new List<Player>();
  public Task<Result<Player, StorageError>> CreatePlayer(Player player)
  {
    if (_memoryPlayers.Any(x => x.Id == player.Id))
    {
      return Task.FromResult<Result<Player, StorageError>>(StorageError.CantStore);
    }
    _memoryPlayers.Add(player);
    return Task.FromResult<Result<Player, StorageError>>(player);
  }

  public Task<Result<Player, StorageError>> GetPlayerById(Guid id)
  {
    int playerIndex = _memoryPlayers.FindIndex(x => x.Id == id);
    if (playerIndex == -1)
    {
      return Task.FromResult<Result<Player, StorageError>>(StorageError.NotFound);
    }
    
    return Task.FromResult<Result<Player, StorageError>>(_memoryPlayers[playerIndex]);
  }

  public Task<Result<Player, StorageError>> UpdatePlayer(Player player)
  {
    int playerIndex = _memoryPlayers.FindIndex(x => x.Id == player.Id && x.HashedSecret == player.HashedSecret);
    if (playerIndex == -1)
    {
      return Task.FromResult<Result<Player, StorageError>>(StorageError.NotFound);
    }
    _memoryPlayers[playerIndex] = player;
    return Task.FromResult<Result<Player, StorageError>>(player);
  }
}