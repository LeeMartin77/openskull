using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.AspNetCore.SignalR;
using OpenSkull.Api.Hubs;
using System.Text.Json;

namespace OpenSkull.Api.Messaging;

public class KafkaWebSocketManager : IWebSocketManager
{
  private readonly IHubContext<PlayerHub> _playerHubContext;
  private readonly IHubContext<GameHub> _gameHubContext;
  private readonly ProducerConfig _producerConfig;
  private readonly ConsumerConfig _consumerConfig;

  private record OpenskullSocketKafkaMessage {
    public Guid Id { get; set; }
    public OpenskullMessage Message { get; set; }
  }

  public KafkaWebSocketManager(
    IHubContext<PlayerHub> playerHubContext, 
    IHubContext<GameHub> gameHubContext,
    ProducerConfig producerConfig,
    ConsumerConfig consumerConfig
    ) {
    _playerHubContext = playerHubContext;
    _gameHubContext = gameHubContext;
    _producerConfig = producerConfig;
    _consumerConfig = consumerConfig;
  }

  // Doing this because I think they're listening to different topics.
  private static string GetKafkaTopic(WebSocketType type) {
    switch (type) {
      case WebSocketType.Game:
        return "Game";
      case WebSocketType.Player:
        return "Player";
      default:
        throw new NotImplementedException();
    }
  }

  public async Task BroadcastToConnectedWebsockets(WebSocketType type, Guid id, OpenskullMessage message)
  {
    using (var producer = new ProducerBuilder<Null, string>(_producerConfig).Build())
    {
      await producer.ProduceAsync(GetKafkaTopic(type), new Message<Null, string> { Value=JsonSerializer.Serialize(new OpenskullSocketKafkaMessage {
        Id = id,
        Message = message
      })});
    }
  }

  public async Task WebsocketMessageSenderThread()
  {
    var gameTopic =  GetKafkaTopic(WebSocketType.Game);
    var playerTopic =  GetKafkaTopic(WebSocketType.Player);
    using (var producer = new ProducerBuilder<Null, string>(_producerConfig).Build())
    {
      await producer.ProduceAsync(gameTopic, new Message<Null, string> { Value=JsonSerializer.Serialize(new OpenskullSocketKafkaMessage {
        Id = Guid.Empty,
        Message = new OpenskullMessage{ Id = Guid.Empty, Activity = "ApiJoined"}
      })});
      await producer.ProduceAsync(playerTopic, new Message<Null, string> { Value=JsonSerializer.Serialize(new OpenskullSocketKafkaMessage {
        Id = Guid.Empty,
        Message = new OpenskullMessage{ Id = Guid.Empty, Activity = "ApiJoined"}
      })});
    }
    using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
    {
      consumer.Subscribe(new string[] { gameTopic, playerTopic });
      while (true)
      {
        var consumeResult = consumer.Consume();
        var message = JsonSerializer.Deserialize<OpenskullSocketKafkaMessage>(consumeResult.Message.Value);
        
        if (message != null && consumeResult.Topic == gameTopic) {
          await _gameHubContext.Clients.Group(message.Id.ToString()).SendCoreAsync("send", new object[]{message.Message});
        }
        if (message != null && consumeResult.Topic == playerTopic) {
          await _playerHubContext.Clients.Group(message.Id.ToString()).SendCoreAsync("send", new object[]{message.Message});
        }
      }
    }
  }
}