using System.Text.Json;
using Confluent.Kafka;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Messaging;
using OpenSkull.Api.Storage;

namespace OpenSkull.Api.Queue;

public class KafkaGameCreationQueue : IGameCreationQueue
{
  private Queue<Guid>[] _gameCreationQueue;
  private readonly GameCreateNew _gameCreateNew;
  private readonly IGameStorage _gameStorage;
  private readonly IWebSocketManager _webSocketManager;
  private readonly ProducerConfig _producerConfig;
  private readonly ConsumerConfig _consumerConfig;

  public KafkaGameCreationQueue(
    GameCreateNew gameCreationFunction, 
    IGameStorage gameStorage, 
    IWebSocketManager webSocketManager,
    ProducerConfig producerConfig,
    ConsumerConfig consumerConfig
    ) {
    int queuesToCreate = GameFunctions.MAX_PLAYERS - GameFunctions.MIN_PLAYERS + 1;
    _gameCreationQueue = new Queue<Guid>[queuesToCreate];
    for (int i = 0; i < queuesToCreate; i++) {
      _gameCreationQueue[i] = new Queue<Guid>();
    }
    _gameStorage = gameStorage;
    _gameCreateNew = gameCreationFunction;
    _webSocketManager = webSocketManager;
    _producerConfig = producerConfig;
    _consumerConfig = consumerConfig;
  }


  private readonly string _GAME_CREATION_TOPIC = "GameCreation";

  private enum GameCreationMessageType {
    Join,
    Leave,
    Find,
    Count
  }

  private record GameCreationMessage {
    public GameCreationMessageType Type { get; set; }
    public Guid PlayerId { get; set; }
    public int GameSize { get; set; }
  }

  private record GameCreationResponseMessage {
    public Guid PlayerId { get; set; }
    public bool ResponseSuccess { get; set; }
    public string ResponseString { get; set; } = "";
  }


  private async Task _SendEventMessage(GameCreationMessage message) {
    using (var producer = new ProducerBuilder<Null, string>(_producerConfig).Build())
    {
      await producer.ProduceAsync(_GAME_CREATION_TOPIC, new Message<Null, string> { Value=JsonSerializer.Serialize(message)});
    }
  }

  public async Task FindPlayerInQueues(Guid playerId) {
    var sendMessage = new GameCreationMessage {
      Type = GameCreationMessageType.Find,
      PlayerId = playerId
    };
    await _SendEventMessage(sendMessage);
  }

  private PlayerQueueStatus _FindPlayersInMemoryQueues(Guid playerId) {
    int queueIndex = Array.FindIndex(_gameCreationQueue, x => x.Contains(playerId));
    if(queueIndex == -1) {
      return new PlayerQueueStatus {
          GameSize = 0,
          QueueSize = 0
        };
    }

    var status = new PlayerQueueStatus {
      GameSize = queueIndex + GameFunctions.MIN_PLAYERS,
      QueueSize = _gameCreationQueue[queueIndex].Count
    };

    return status;
  }

  public async Task LeaveQueues(Guid playerId) {
    var sendMessage = new GameCreationMessage {
      Type = GameCreationMessageType.Leave,
      PlayerId = playerId
    };
    await _SendEventMessage(sendMessage);
  }

  private void _LeaveMemoryQueues(Guid playerId) {
    int queueIndex = Array.FindIndex(_gameCreationQueue, x => x.Contains(playerId));
    if(queueIndex == -1) {
      return;
    }
    _gameCreationQueue[queueIndex] = new Queue<Guid>(_gameCreationQueue[queueIndex].Where(x => x != playerId));
  }

  public async Task JoinGameQueue(Guid playerId, int gameSize)
  {
    var sendMessage = new GameCreationMessage {
      Type = GameCreationMessageType.Join,
      GameSize = gameSize,
      PlayerId = playerId
    };
    await _SendEventMessage(sendMessage);
  }

  private async Task<Result<bool, QueueJoinError>> _JoinMemoryGameQueue(Guid playerId, int gameSize)
  {
    int queueIndex = gameSize - GameFunctions.MIN_PLAYERS;
    if (queueIndex < 0 || queueIndex > _gameCreationQueue.Length - 1) {
      return QueueJoinError.OutsideGameSize;
    }
    if (_gameCreationQueue.Any(x => x.Contains(playerId))) {
      return QueueJoinError.AlreadyInQueue;
    }
    if (_gameCreationQueue[queueIndex].Count >= gameSize - 1) {
      
      var queuedPlayerIds = new List<Guid>();
      while(queuedPlayerIds.Count < gameSize -1) {
        queuedPlayerIds.Add(_gameCreationQueue[queueIndex].Dequeue());
      }
      var playerIds = queuedPlayerIds.ToList();
      playerIds.Add(playerId);
      var createdGame = _gameCreateNew(playerIds.ToArray());
      if (createdGame.IsFailure) {
        queuedPlayerIds.ForEach(x => _gameCreationQueue[queueIndex].Enqueue(x));
        Console.Error.WriteLine(QueueJoinError.CreationError);
      }
      var storedGame = await _gameStorage.StoreNewGame(createdGame.Value);
      if (storedGame.IsFailure) {
        queuedPlayerIds.ForEach(x => _gameCreationQueue[queueIndex].Enqueue(x));
        Console.Error.WriteLine(QueueJoinError.StorageError);
      }
      try {
        await Task.WhenAll(storedGame.Value.Game.PlayerIds
          .Select(id => 
              _webSocketManager.BroadcastToConnectedWebsockets(WebSocketType.Player, id, new OpenskullMessage { 
                Id = storedGame.Value.Id, 
                Activity = "GameCreated" 
              })
          )
        );
      } catch {
          //Drowning any weird exceptions
      }
      return true;
    }
    _gameCreationQueue[queueIndex].Enqueue(playerId);
    return false;
  }

  public async Task GameMasterThread() {
    using (var producer = new ProducerBuilder<Null, string>(_producerConfig).Build()) 
    {
      Console.WriteLine("GameManager Connecting to KafKa");
      await producer.ProduceAsync(_GAME_CREATION_TOPIC, new Message<Null, string> { Value=JsonSerializer.Serialize(new GameCreationMessage{ })});
    }
    using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
    {
      consumer.Subscribe(_GAME_CREATION_TOPIC);
      while (true) {

        var singleMessage = consumer.Consume().Message.Value;
        if (singleMessage is not null) {
          var singleParsedMessage = JsonSerializer.Deserialize<GameCreationMessage>(singleMessage);
          Console.WriteLine(singleParsedMessage);
          if (singleParsedMessage is not null) {
            // This probably needs a relook
            switch (singleParsedMessage.Type) {
              case GameCreationMessageType.Join:
                var joinResult = await _JoinMemoryGameQueue(singleParsedMessage.PlayerId, singleParsedMessage.GameSize);
                if (joinResult.IsFailure) {
                  await _webSocketManager.BroadcastToConnectedWebsockets(WebSocketType.Player, singleParsedMessage.PlayerId, new OpenskullMessage { 
                    Id = singleParsedMessage.PlayerId, 
                    Activity = "QueueJoinFailure",
                    Details = JsonSerializer.Serialize(new { GameSize = singleParsedMessage.GameSize })
                    });
                  return;
                }
                break;
              case GameCreationMessageType.Find:
                var findResult = _FindPlayersInMemoryQueues(singleParsedMessage.PlayerId);
                await _webSocketManager.BroadcastToConnectedWebsockets(WebSocketType.Player, singleParsedMessage.PlayerId, new OpenskullMessage { 
                    Id = singleParsedMessage.PlayerId, 
                    Activity = "QueueStatus",
                    Details = JsonSerializer.Serialize(findResult)
                  });
                break;
              case GameCreationMessageType.Leave:
                _LeaveMemoryQueues(singleParsedMessage.PlayerId);
                break;
              default:
                throw new NotImplementedException("Unknown Game Creation Message Type");
            }
          }
        }
      }
    }
  }
}