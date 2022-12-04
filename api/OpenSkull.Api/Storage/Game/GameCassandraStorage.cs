using Cassandra;
using Cassandra.Mapping;
using System.Text.Json;
using OpenSkull.Api.DTO;

namespace OpenSkull.Api.Storage;

public class GameCassandraStorage : IGameStorage {

  private class CassandraGameStorage {
    public Guid id { get; set; }
    public string version_tag { get; set; } = "";
    public string game { get; set; } = "";
    public Guid[] player_ids { get; set; }
    public DateTime last_updated { get; set; }
    public CassandraGameStorage() {
      id = Guid.Empty;
      version_tag = "";
      game = "";
      player_ids = new Guid[0];
      last_updated = DateTime.UtcNow;
    }
    public CassandraGameStorage(GameStorage storage) {
      id = storage.Id;
      version_tag = storage.VersionTag;
      game = JsonSerializer.Serialize(storage.Game);
      player_ids = storage.Game.PlayerIds;
      last_updated = storage.LastUpdated;
    }
    public static GameStorage ToGameStorage(CassandraGameStorage csstorage) {
      return new GameStorage {
        Id = csstorage.id,
        VersionTag = csstorage.version_tag,
        Game = JsonSerializer.Deserialize<Game>(csstorage.game),
        LastUpdated = csstorage.last_updated
      };
    }
  }

  private readonly Cluster _cluster;
  private readonly string _keyspace;
  public GameCassandraStorage(Cluster cluster, string keyspace){
    _cluster = cluster;
    _keyspace = keyspace;
  }

  public async Task<Result<GameStorage, StorageError>> StoreNewGame(Game game)
  {
    try {
      var newGameStorage = new GameStorage {
        Id = Guid.NewGuid(),
        VersionTag = Guid.NewGuid().ToString(),
        Game = game,
        LastUpdated = DateTime.UtcNow
      };
      using (var session = await _cluster.ConnectAsync(_keyspace))
      {
        var csgame = new CassandraGameStorage(newGameStorage);
        var ps = await session.PrepareAsync(@"INSERT INTO games 
        (id, version_tag, game, player_ids, last_updated) 
        VALUES 
        (?, ?, ?, ?, ?)");

        var statement = ps.Bind(
          csgame.id,
          csgame.version_tag,
          csgame.game,
          csgame.player_ids,
          csgame.last_updated
          );

        await session.ExecuteAsync(statement);
      }
      return newGameStorage;
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }

  public async Task<Result<GameStorage, StorageError>> GetGameById(Guid gameId)
  {
    try {
      using (var session = await _cluster.ConnectAsync(_keyspace))
      {
        IMapper mapper = new Mapper(session);
        try {
          CassandraGameStorage csgame = await mapper.SingleAsync<CassandraGameStorage>("SELECT * FROM games WHERE id = ?", gameId);
          return CassandraGameStorage.ToGameStorage(csgame);
        } catch (Exception) {
          return StorageError.NotFound;
        }
      }
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }


  public async Task<Result<GameStorage[], StorageError>> SearchGames(GameSearchParameters parameters) 
  {
    try {
      using (var session = await _cluster.ConnectAsync(_keyspace))
      {
        IMapper mapper = new Mapper(session);
        try {
          // TODO: Need to implement paging?
          var csgame = await mapper.FetchAsync<CassandraGameStorage>(
            "SELECT * FROM games WHERE player_ids contains ?", parameters.PlayerId);
          return csgame.Select(CassandraGameStorage.ToGameStorage).ToArray();
        } catch (Exception) {
          return StorageError.NotFound;
        }
      }
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }

  public async Task<Result<GameStorage, StorageError>> UpdateGame(GameStorage gameStorage)
  {
    try {
      using (var session = await _cluster.ConnectAsync(_keyspace))
      {
        var csgame = new CassandraGameStorage(gameStorage);
        var ps = await session.PrepareAsync(@"UPDATE games SET
        version_tag = ?, game = ?, player_ids = ?, last_updated = ?
        WHERE id = ?");

        var statement = ps.Bind(
          csgame.version_tag,
          csgame.game,
          csgame.player_ids,
          csgame.last_updated,
          csgame.id
          );

        await session.ExecuteAsync(statement);
        IMapper mapper = new Mapper(session);
        try {
          CassandraGameStorage stored = await mapper.SingleAsync<CassandraGameStorage>(
            "SELECT * FROM games WHERE id = ?", gameStorage.Id);
          return CassandraGameStorage.ToGameStorage(stored);
        } catch (Exception) {
          return StorageError.NotFound;
        }
      }
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }
}
