namespace OpenSkull.Api.Storage;

public class RoomMemoryStorage : IRoomStorage
{
  private Dictionary<string, List<Guid>> _roomDictionary = new Dictionary<string, List<Guid>>();
  public Task AddPlayerIdToRoom(string roomId, Guid playerId)
  {
    if (_roomDictionary.ContainsKey(roomId))
    {
      _roomDictionary[roomId].Add(playerId);
      _roomDictionary[roomId] = _roomDictionary[roomId].Distinct().ToList();
    }
    else 
    {
      _roomDictionary.Add(roomId, new List<Guid>() { playerId });
    }
    return Task.CompletedTask;
  }

  public Task<List<Guid>> GetRoomPlayerIds(string roomId)
  {
    if (_roomDictionary.ContainsKey(roomId))
    {
      return Task.FromResult(_roomDictionary[roomId]);
    }
    return Task.FromResult(new List<Guid>());
  }

  public Task RemovePlayerIdFromRoom(string roomId, Guid playerId)
  {
    if (_roomDictionary.ContainsKey(roomId))
    {
      _roomDictionary[roomId].Remove(playerId);
    }
    return Task.CompletedTask;
  }

  public Task<string[]> RemovePlayerIdFromAllRooms(Guid playerId)
  {
    List<string> roomsRemovedFrom = new List<string>();
    foreach (var room in _roomDictionary) {
      if(room.Value.Remove(playerId))
      {
        _roomDictionary[room.Key] = room.Value;
        roomsRemovedFrom.Add(room.Key);
      }
    }
    return Task.FromResult(roomsRemovedFrom.ToArray());
  }
}