namespace OpenSkull.Api.Storage;

public record Player 
{
  public Guid Id { get; set; }
  public byte[] HashedSecret { get; set; } = new byte[0];
  public string Salt { get; set; } = "";
  public string Nickname { get; set; } = "";
}


public interface IPlayerStorage 
{
  Task<Result<Player, StorageError>> CreatePlayer(Player player);
  Task<Result<Player, StorageError>> GetPlayerById(Guid id);
  Task<Result<Player, StorageError>> UpdatePlayer(Player player);
}