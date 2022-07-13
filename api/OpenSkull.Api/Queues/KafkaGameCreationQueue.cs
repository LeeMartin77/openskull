// using System.Text.Json;
// using Confluent.Kafka;
// using OpenSkull.Api.Functions;
// using OpenSkull.Api.Messaging;
// using OpenSkull.Api.Storage;

// namespace OpenSkull.Api.Queue;

// public class KafkaGameCreationQueue : IGameCreationQueue
// {
//   private Queue<Guid>[] _gameCreationQueue;
//   private readonly GameCreateNew _gameCreateNew;
//   private readonly IGameStorage _gameStorage;
//   private readonly IWebSocketManager _webSocketManager;
//   private readonly ProducerConfig _producerConfig;
//   private readonly ConsumerConfig _consumerConfig;

//   public KafkaGameCreationQueue(
//     GameCreateNew gameCreationFunction, 
//     IGameStorage gameStorage, 
//     IWebSocketManager webSocketManager,
//     ProducerConfig producerConfig,
//     ConsumerConfig consumerConfig
//     ) {
//     int queuesToCreate = GameFunctions.MAX_PLAYERS - GameFunctions.MIN_PLAYERS + 1;
//     _gameCreationQueue = new Queue<Guid>[queuesToCreate];
//     for (int i = 0; i < queuesToCreate; i++) {
//       _gameCreationQueue[i] = new Queue<Guid>();
//     }
//     _gameStorage = gameStorage;
//     _gameCreateNew = gameCreationFunction;
//     _webSocketManager = webSocketManager;
//     _producerConfig = producerConfig;
//     _consumerConfig = consumerConfig;
//   }


//   private readonly string _GAME_CREATION_TOPIC = "GameCreation";
//   private readonly string _GAME_CREATION_RESPONSE_TOPIC = "GameCreationResponse";

//   private enum GameCreationMessageType {
//     Join,
//     Leave,
//     Find,
//     Count
//   }

//   private record GameCreationMessage {
//     public Guid CorrelationId { get; set; }
//     public GameCreationMessageType Type { get; set; }
//     public Guid PlayerId { get; set; }
//     public int GameSize { get; set; }
//   }

//   private record GameCreationResponseMessage {
//     public Guid CorrelationId { get; set; }
//     public bool ResponseSuccess { get; set; }
//     public string ResponseString { get; set; } = "";
//   }


//   private async Task<GameCreationResponseMessage> _SendReceiveCorrelatedMessages(GameCreationMessage message) {
//     GameCreationResponseMessage? responseMessage = null;
//     var correlationId = Guid.NewGuid();
//     message = message with {
//       CorrelationId = correlationId
//     };
//     using (var producer = new ProducerBuilder<Null, string>(_producerConfig).Build())
//     using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
//     {
//       consumer.Subscribe(_GAME_CREATION_RESPONSE_TOPIC);
//       await producer.ProduceAsync(_GAME_CREATION_TOPIC, new Message<Null, string> { Value=JsonSerializer.Serialize(message)});
      
//       while (responseMessage is null) {
//         var singleMessage = consumer.Consume().Message.Value;
//         if (singleMessage is not null) {
//           var singleParsedMessage = JsonSerializer.Deserialize<GameCreationResponseMessage>(singleMessage);
//           if (singleParsedMessage is not null && singleParsedMessage.CorrelationId == correlationId) {
//             responseMessage = singleParsedMessage;
//           }
//         }
//       }
//     }
//     return responseMessage;
//   }

//   private async Task _SendResponseMessage(GameCreationResponseMessage message) {
//     using (var producer = new ProducerBuilder<Null, string>(_producerConfig).Build())
//     {
//       await producer.ProduceAsync(_GAME_CREATION_RESPONSE_TOPIC, new Message<Null, string> { Value=JsonSerializer.Serialize(message)});
//     }
//   }

//   public async Task<Result<PlayerQueueStatus, PlayerQueueStatusError>> FindPlayerInQueues(Guid playerId) {
//     var sendMessage = new GameCreationMessage {
//       Type = GameCreationMessageType.Find,
//       PlayerId = playerId
//     };
//     var message = await _SendReceiveCorrelatedMessages(sendMessage);
//     if (message.ResponseSuccess) {
//       return JsonSerializer.Deserialize<PlayerQueueStatus>(message.ResponseString);
//     } else {
//       return Enum.Parse<PlayerQueueStatusError>(message.ResponseString);
//     }
//   }

//   private Result<PlayerQueueStatus, PlayerQueueStatusError> _FindPlayersInMemoryQueues(Guid playerId) {
//     int queueIndex = Array.FindIndex(_gameCreationQueue, x => x.Contains(playerId));
//     if(queueIndex == -1) {
//       return PlayerQueueStatusError.PlayerNotInAnyQueues;
//     }

//     var status = new PlayerQueueStatus {
//       GameSize = queueIndex + GameFunctions.MIN_PLAYERS,
//       QueueSize = _gameCreationQueue[queueIndex].Count
//     };

//     return status;
//   }

//   public async Task<Result<bool, PlayerQueueLeaveError>> LeaveQueues(Guid playerId) {
//     var sendMessage = new GameCreationMessage {
//       Type = GameCreationMessageType.Leave,
//       PlayerId = playerId
//     };
//     var message = await _SendReceiveCorrelatedMessages(sendMessage);
//     if (message.ResponseSuccess) {
//       return true;
//     } else {
//       return Enum.Parse<PlayerQueueLeaveError>(message.ResponseString);
//     }
//   }

//   private Result<bool, PlayerQueueLeaveError> _LeaveMemoryQueues(Guid playerId) {
//     int queueIndex = Array.FindIndex(_gameCreationQueue, x => x.Contains(playerId));
//     if(queueIndex == -1) {
//       return PlayerQueueLeaveError.PlayerNotInAnyQueues;
//     }
//     _gameCreationQueue[queueIndex] = new Queue<Guid>(_gameCreationQueue[queueIndex].Where(x => x != playerId));
//     return true;
//   }

//   public async Task<Result<int, QueueError>> PlayersInQueue(int gameSize)
//   {
//     var sendMessage = new GameCreationMessage {
//       Type = GameCreationMessageType.Count,
//       GameSize = gameSize
//     };
//     var message = await _SendReceiveCorrelatedMessages(sendMessage);
//     if (message.ResponseSuccess) {
//       return int.Parse(message.ResponseString);
//     } else {
//       return Enum.Parse<QueueError>(message.ResponseString);
//     }
//   }

//   private Result<int, QueueError> _PlayersInMemoryQueue(int gameSize)
//   {
//     return _gameCreationQueue[gameSize - GameFunctions.MIN_PLAYERS].Count;
//   }

//   public async Task<Result<bool, QueueJoinError>> JoinGameQueue(Guid playerId, int gameSize)
//   {
//     var sendMessage = new GameCreationMessage {
//       Type = GameCreationMessageType.Join,
//       GameSize = gameSize,
//       PlayerId = playerId
//     };
//     var message = await _SendReceiveCorrelatedMessages(sendMessage);
//     if (message.ResponseSuccess) {
//       return bool.Parse(message.ResponseString);
//     } else {
//       return Enum.Parse<QueueJoinError>(message.ResponseString);
//     }
//   }

//   private async Task<Result<bool, QueueJoinError>> _JoinMemoryGameQueue(Guid playerId, int gameSize)
//   {
//     int queueIndex = gameSize - GameFunctions.MIN_PLAYERS;
//     if (queueIndex < 0 || queueIndex > _gameCreationQueue.Length - 1) {
//       return QueueJoinError.OutsideGameSize;
//     }
//     if (_gameCreationQueue.Any(x => x.Contains(playerId))) {
//       return QueueJoinError.AlreadyInQueue;
//     }
//     if (_gameCreationQueue[queueIndex].Count >= gameSize - 1) {
      
//       var queuedPlayerIds = new List<Guid>();
//       while(queuedPlayerIds.Count < gameSize -1) {
//         queuedPlayerIds.Add(_gameCreationQueue[queueIndex].Dequeue());
//       }
//       var playerIds = queuedPlayerIds.ToList();
//       playerIds.Add(playerId);
//       var createdGame = _gameCreateNew(playerIds.ToArray());
//       if (createdGame.IsFailure) {
//         queuedPlayerIds.ForEach(x => _gameCreationQueue[queueIndex].Enqueue(x));
//         Console.Error.WriteLine(QueueJoinError.CreationError);
//       }
//       var storedGame = await _gameStorage.StoreNewGame(createdGame.Value);
//       if (storedGame.IsFailure) {
//         queuedPlayerIds.ForEach(x => _gameCreationQueue[queueIndex].Enqueue(x));
//         Console.Error.WriteLine(QueueJoinError.StorageError);
//       }
//       try {
//         await Task.WhenAll(storedGame.Value.Game.PlayerIds
//           .Select(id => 
//               _webSocketManager.BroadcastToConnectedWebsockets(WebSocketType.Player, id, new OpenskullMessage { Id = storedGame.Value.Id, Activity = "GameCreated" })
//           )
//         );
//       } catch {
//           //Drowning any weird exceptions
//       }
//       return true;
//     }
//     _gameCreationQueue[queueIndex].Enqueue(playerId);
//     return false;
//   }

//   public async Task GameMasterThread() {
//     using (var producer = new ProducerBuilder<Null, string>(_producerConfig).Build()) 
//     {
//       Console.WriteLine("GameManager Connecting to KafKa");
//       await producer.ProduceAsync(_GAME_CREATION_TOPIC, new Message<Null, string> { Value=JsonSerializer.Serialize(new GameCreationMessage{ })});
//       await producer.ProduceAsync(_GAME_CREATION_RESPONSE_TOPIC, new Message<Null, string> { Value=JsonSerializer.Serialize(new GameCreationResponseMessage{ })});
//     }
//     using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
//     {
//       consumer.Subscribe(_GAME_CREATION_TOPIC);
//       while (true) {

//         var singleMessage = consumer.Consume().Message.Value;
//         if (singleMessage is not null) {
//           var singleParsedMessage = JsonSerializer.Deserialize<GameCreationMessage>(singleMessage);
//           if (singleParsedMessage is not null) {
//             // This probably needs a relook
//             switch (singleParsedMessage.Type) {
//               case GameCreationMessageType.Join:
//                 var joinResult = await _JoinMemoryGameQueue(singleParsedMessage.PlayerId, singleParsedMessage.GameSize);
//                 if (joinResult.IsSuccess) {
//                   await _SendResponseMessage(new GameCreationResponseMessage {
//                     ResponseSuccess = true,
//                     ResponseString = JsonSerializer.Serialize(joinResult.Value)
//                   });
//                 } else {
//                   await _SendResponseMessage(new GameCreationResponseMessage {
//                     ResponseSuccess = false,
//                     ResponseString = JsonSerializer.Serialize(joinResult.Error)
//                   });
//                 }
//                 break;
//               case GameCreationMessageType.Find:
//                 var findResult = _FindPlayersInMemoryQueues(singleParsedMessage.PlayerId);
//                 if (findResult.IsSuccess) {
//                   await _SendResponseMessage(new GameCreationResponseMessage {
//                     ResponseSuccess = true,
//                     ResponseString = JsonSerializer.Serialize(findResult.Value)
//                   });
//                 } else {
//                   await _SendResponseMessage(new GameCreationResponseMessage {
//                     ResponseSuccess = false,
//                     ResponseString = JsonSerializer.Serialize(findResult.Error)
//                   });
//                 }
//                 break;
//               case GameCreationMessageType.Leave:
//                 var leaveResult = _LeaveMemoryQueues(singleParsedMessage.PlayerId);
//                 if (leaveResult.IsSuccess) {
//                   await _SendResponseMessage(new GameCreationResponseMessage {
//                     ResponseSuccess = true,
//                     ResponseString = JsonSerializer.Serialize(leaveResult.Value)
//                   });
//                 } else {
//                   await _SendResponseMessage(new GameCreationResponseMessage {
//                     ResponseSuccess = false,
//                     ResponseString = JsonSerializer.Serialize(leaveResult.Error)
//                   });
//                 }
//                 break;
//               case GameCreationMessageType.Count:
//                 var countResult = _PlayersInMemoryQueue(singleParsedMessage.GameSize);
//                 if (countResult.IsSuccess) {
//                   await _SendResponseMessage(new GameCreationResponseMessage {
//                     ResponseSuccess = true,
//                     ResponseString = JsonSerializer.Serialize(countResult.Value)
//                   });
//                 } else {
//                   await _SendResponseMessage(new GameCreationResponseMessage {
//                     ResponseSuccess = false,
//                     ResponseString = JsonSerializer.Serialize(countResult.Error)
//                   });
//                 }
//                 break;
//               default:
//                 throw new NotImplementedException("Unknown Game Creation Message Type");
//             }
//           }
//         }
//       }
//     }
//   }
// }