using System.Linq;
using System.Text.Json;
using Dapper;
using Npgsql;

namespace OpenSkull.Api.Storage;

public class PlayerPostgresStorage : IPlayerStorage
{
  private readonly string _connectionString;
  public PlayerPostgresStorage(string connectionString){
    _connectionString = connectionString;
  }

  private class PostgresPlayerStorageItem {
    public Guid id { get; set; }
    public byte[] hashed_secret { get; set; } = new byte[0];
    public string salt { get; set; } = "";
    public string nickname { get; set; }
    public PostgresPlayerStorageItem() {
      id = Guid.Empty;
      hashed_secret = new byte[0];
      salt = "";
      nickname = "";
    }
    public PostgresPlayerStorageItem(Player player) {
      id = player.Id;
      hashed_secret = player.HashedSecret;
      salt = player.Salt;
      nickname = player.Nickname;
    }
    public static Player ToPlayer(PostgresPlayerStorageItem pgstorage) {
      return new Player {
        Id = pgstorage.id,
        HashedSecret = pgstorage.hashed_secret,
        Salt = pgstorage.salt,
        Nickname = pgstorage.nickname
      };
    }
  }

  public async Task<Result<Player, StorageError>> CreatePlayer(Player player)
  {
    try {
      PostgresPlayerStorageItem newPlayerStorage = new PostgresPlayerStorageItem(player);
      using (var connection = new NpgsqlConnection(_connectionString))
      {
        await connection.ExecuteAsync(@"
          INSERT INTO players 
            (id, hashed_secret, salt, nickname) 
          VALUES (@id, @hashed_secret, @salt, @nickname)", 
        newPlayerStorage);
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
      using (var connection = new NpgsqlConnection(_connectionString))
      {
        var result = await connection.QueryFirstOrDefaultAsync<PostgresPlayerStorageItem>(
          @"SELECT id, hashed_secret, salt, nickname
            FROM players WHERE id = @player_id"
          , new { player_id = id }
        );
        if (result is null) {
          return StorageError.NotFound;
        }
        return PostgresPlayerStorageItem.ToPlayer(result);
      }
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }

  public async Task<Result<Player, StorageError>> UpdatePlayer(Player player)
  {
    try {
      using (var connection = new NpgsqlConnection(_connectionString))
      {
        var result = await connection.ExecuteAsync(
          @"UPDATE players 
            SET nickname = @nickname
            WHERE id = @id", new PostgresPlayerStorageItem(player));
        var reretrieve = await GetPlayerById(player.Id);
        if (reretrieve.IsFailure) {
          return reretrieve.Error;
        }
        return reretrieve.Value;
      }
    } catch (Exception ex) {
      Console.Error.WriteLine(ex.Message);
      return StorageError.SystemError;
    }
  }
}