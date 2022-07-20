using System.Linq;

namespace OpenSkull.Api.Storage;

public class PlayerMemoryStorage : IPlayerStorage
{
  private Dictionary<Guid, Player> _memoryPlayers = new Dictionary<Guid, Player>();
  
  public Task<Result<Player, StorageError>> CreatePlayer(Player player)
  {
    if (_memoryPlayers.Keys.Any(x => x == player.Id))
    {
      return Task.FromResult<Result<Player, StorageError>>(StorageError.CantStore);
    }
    _memoryPlayers.Add(player.Id, player);
    return Task.FromResult<Result<Player, StorageError>>(player);
  }

  public Task<Result<Player, StorageError>> GetPlayerById(Guid id)
  {
    Player? player = null;
    if (!_memoryPlayers.TryGetValue(id, out player)) {
      return Task.FromResult<Result<Player, StorageError>>(StorageError.NotFound);
    }
    return Task.FromResult<Result<Player, StorageError>>(player);
  }

  public Task<Result<Player, StorageError>> UpdatePlayer(Player player)
  {
    Player? storedPlayer = null;
    if (_memoryPlayers.TryGetValue(player.Id, out storedPlayer) && storedPlayer.HashedSecret == player.HashedSecret) {
      _memoryPlayers[player.Id] = player;
      return Task.FromResult<Result<Player, StorageError>>(player);
    }
    return Task.FromResult<Result<Player, StorageError>>(StorageError.NotFound);
  }
}