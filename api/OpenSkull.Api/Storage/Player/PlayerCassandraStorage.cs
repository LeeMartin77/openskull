using Cassandra;
using Cassandra.Mapping;
namespace OpenSkull.Api.Storage;

public class PlayerCassandraStorage : IPlayerStorage {
  private class CassandraPlayerStorageItem {
    public Guid id { get; set; }
    public byte[] hashed_secret { get; set; } = new byte[0];
    public string salt { get; set; } = "";
    public string nickname { get; set; }
    public CassandraPlayerStorageItem() {
      id = Guid.Empty;
      hashed_secret = new byte[0];
      salt = "";
      nickname = "";
    }
    public CassandraPlayerStorageItem(Player player) {
      id = player.Id;
      hashed_secret = player.HashedSecret;
      salt = player.Salt;
      nickname = player.Nickname;
    }
    public static Player ToPlayer(CassandraPlayerStorageItem csstorage) {
      return new Player {
        Id = csstorage.id,
        HashedSecret = csstorage.hashed_secret,
        Salt = csstorage.salt,
        Nickname = csstorage.nickname
      };
    }
  }

  private readonly Cluster _cluster;
  private readonly string _keyspace;

  public PlayerCassandraStorage(Cluster cluster, string keyspace){
    _cluster = cluster;
    _keyspace = keyspace;
  }

  public async Task<Result<Player, StorageError>> CreatePlayer(Player player)
  {
    try {
      var newPlayer = new CassandraPlayerStorageItem(player);
      using (var session = await _cluster.ConnectAsync(_keyspace))
      {
        var ps = await session.PrepareAsync(@"INSERT INTO players 
        (id, hashed_secret, salt, nickname)
        VALUES 
        (?, ?, ?, ?)");

        var statement = ps.Bind(
          newPlayer.id,
          newPlayer.hashed_secret,
          newPlayer.salt,
          newPlayer.nickname
          );

        await session.ExecuteAsync(statement);
      }
      return player;
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }

  public async Task<Result<Player, StorageError>> GetPlayerById(Guid id)
  {
    try {
      using (var session = await _cluster.ConnectAsync(_keyspace))
      {
        IMapper mapper = new Mapper(session);
        try {
          CassandraPlayerStorageItem csgame = await mapper.SingleAsync<CassandraPlayerStorageItem>(
            "SELECT * FROM players WHERE id = ?", id
          );
          return CassandraPlayerStorageItem.ToPlayer(csgame);
        } catch (Exception) {
          return StorageError.NotFound;
        }
      }
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }

  public async Task<Result<Player[], StorageError>> GetPlayersByIds(Guid[] ids)
  {
    try {
      using (var session = await _cluster.ConnectAsync(_keyspace))
      {
        IMapper mapper = new Mapper(session);
        try {
          var playerTasks = ids.Select(id => mapper.SingleAsync<CassandraPlayerStorageItem>(
            "SELECT * FROM players WHERE id = ?", id
          ));
          var players = await Task.WhenAll(playerTasks);
          return players.Where(x => x != null).Select(CassandraPlayerStorageItem.ToPlayer).ToArray();
        } catch (Exception) {
          return StorageError.NotFound;
        }
      }
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }

  public async Task<Result<Player, StorageError>> UpdatePlayer(Player player)
  {
    try {
      using (var session = await _cluster.ConnectAsync(_keyspace))
      {
        var ps = await session.PrepareAsync(@"UPDATE players 
        SET nickname = ?
        WHERE id = ?");

        var statement = ps.Bind(
          player.Nickname,
          player.Id
        );

        await session.ExecuteAsync(statement);
      }
      return await GetPlayerById(player.Id);
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }
}