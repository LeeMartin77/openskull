using StackExchange.Redis;

namespace OpenSkull.Api.Storage;

public class RoomRedisStorage : IRoomStorage
{
  private readonly IConnectionMultiplexer _redis;
  private static string ROOM_COLLECTION = "ALL_REDIS_ROOMS";
  private static string ROOM_PREFIX = "ROOM_";

  public RoomRedisStorage(IConnectionMultiplexer redisConnection) {
    _redis = redisConnection;
  }

  public async Task AddPlayerIdToRoom(string roomId, Guid playerId)
  {
    var db = _redis.GetDatabase();
    await db.SetAddAsync(ROOM_COLLECTION, ROOM_PREFIX+roomId);
    await db.SetAddAsync(ROOM_PREFIX+roomId, playerId.ToString());
  }

  public async Task<List<Guid>> GetRoomPlayerIds(string roomId)
  {
    var db = _redis.GetDatabase();
    var set = await db.SetMembersAsync(ROOM_PREFIX+roomId);
    if (set is null) {
      return new List<Guid>();
    }
    return set.Select(x => Guid.Parse(x.ToString())).ToList();
  }

  public async Task RemovePlayerIdFromRoom(string roomId, Guid playerId)
  {
    var db = _redis.GetDatabase();
    await db.SetRemoveAsync(ROOM_PREFIX+roomId, playerId.ToString());
  }

  public async Task<string[]> RemovePlayerIdFromAllRooms(Guid playerId)
  {
    var db = _redis.GetDatabase();
    var rooms = await db.SetMembersAsync(ROOM_COLLECTION);
    if (rooms is not null) {
      var roomTasks = await Task.WhenAll(rooms.Select(async room => {
        return (room.ToString().Replace(ROOM_PREFIX, ""), await db.SetRemoveAsync(room.ToString(), playerId.ToString()));
      }));
      return roomTasks.Where(x => x.Item2).Select(x => x.Item1).ToArray();
    }
    return new string[0];
  }
}