namespace OpenSkull.Api.Storage;

public interface IRoomStorage 
{
  Task AddPlayerIdToRoom(string roomId, Guid playerId);
  Task RemovePlayerIdFromRoom(string roomId, Guid playerId);
  Task<string[]> RemovePlayerIdFromAllRooms(Guid playerId);
  Task<List<Guid>> GetRoomPlayerIds(string roomId);
}