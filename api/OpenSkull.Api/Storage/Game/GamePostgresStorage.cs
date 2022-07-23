using OpenSkull.Api.DTO;
using System.Text.Json;
using Npgsql;
using Dapper;

namespace OpenSkull.Api.Storage;

public class GamePostgresStorage : IGameStorage {
  private readonly string _connectionString;
  public GamePostgresStorage(string connectionString){
    _connectionString = connectionString;
  }

  private class PostgresGameStorage {
    public Guid id { get; set; }
    public string version_tag { get; set; } = "";
    public string game { get; set; } = "";
    public Guid[] player_ids { get; set; }
    public PostgresGameStorage() {
      id = Guid.Empty;
      version_tag = "";
      game = "";
      player_ids = new Guid[0];
    }
    public PostgresGameStorage(GameStorage storage) {
      id = storage.Id;
      version_tag = storage.VersionTag;
      game = JsonSerializer.Serialize(storage.Game);
      player_ids = storage.Game.PlayerIds;
    }
    public static GameStorage ToGameStorage(PostgresGameStorage pgstorage) {
      return new GameStorage {
        Id = pgstorage.id,
        VersionTag = pgstorage.version_tag,
        Game = JsonSerializer.Deserialize<Game>(pgstorage.game)
      };
    }
  }

  private record PostgresSearchParameters {
    public Guid player_id { get; set; }
  }

  public async Task<Result<GameStorage, StorageError>> StoreNewGame(Game game)
  {
    try {
      var newGameStorage = new GameStorage {
        Id = Guid.NewGuid(),
        VersionTag = Guid.NewGuid().ToString(),
        Game = game
      };
      using (var connection = new NpgsqlConnection(_connectionString))
      {
        var pggame = new PostgresGameStorage(newGameStorage);
        await connection.ExecuteAsync("INSERT INTO games (id, version_tag, game, player_ids) VALUES (@id, @version_tag, @game::json, @player_ids)", pggame);
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
      using (var connection = new NpgsqlConnection(_connectionString))
      {
        var result = await connection.QueryFirstOrDefaultAsync<PostgresGameStorage>("SELECT id, version_tag, game FROM games WHERE id = @game_id", new { game_id = gameId });
        if (result is null) {
          return StorageError.NotFound;
        }
        return PostgresGameStorage.ToGameStorage(result);
      }
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }


  public async Task<Result<GameStorage[], StorageError>> SearchGames(GameSearchParameters parameters) 
  {
    try {
      var pgparameters = new PostgresSearchParameters {
        player_id = parameters.PlayerId
      };
      using (var connection = new NpgsqlConnection(_connectionString))
      {
        var result = await connection.QueryAsync<PostgresGameStorage>(
          @"SELECT id, version_tag, game 
            FROM games
            WHERE @player_id = ANY (player_ids)"
          , pgparameters);
        if (result is null) {
          return StorageError.NotFound;
        }
        // This can absolutely be done in SQL but I'm fighting it right now
        return result.Select(PostgresGameStorage.ToGameStorage).ToArray();
      }
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }

  public async Task<Result<GameStorage, StorageError>> UpdateGame(GameStorage gameStorage)
  {
    try {
      using (var connection = new NpgsqlConnection(_connectionString))
      {
        var result = await connection.ExecuteAsync("UPDATE games SET game = @game::json, version_tag = gen_random_uuid()::text WHERE id = @id AND version_tag = @version_tag", new PostgresGameStorage(gameStorage));
        var reretrieve = await connection.QueryFirstAsync<PostgresGameStorage>("SELECT id, version_tag, game FROM games WHERE id = @id", new PostgresGameStorage(gameStorage));
        if (reretrieve is null) {
          return StorageError.NotFound;
        }
        if (result == 0) {
          if (reretrieve.version_tag != gameStorage.VersionTag) {
            return StorageError.VersionMismatch;
          }
          return StorageError.SystemError;
        }
        return PostgresGameStorage.ToGameStorage(reretrieve);
      }
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }
}
