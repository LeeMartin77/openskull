using Confluent.Kafka;
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

  public async Task BroadcastToConnectedWebsockets(WebSocketType type, Guid id, OpenskullMessage message)
  {
    using (var producer = new ProducerBuilder<Null, string>(_producerConfig).Build())
    {
      
      await producer.ProduceAsync(type.ToString(), new Message<Null, string> { Value=JsonSerializer.Serialize(new OpenskullSocketKafkaMessage {
        Id = id,
        Message = message
      })});
    }
  }

  public async Task WebsocketMessageSenderThread()
  {
    using (var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build())
    {
      var gameTopic =  WebSocketType.Game.ToString();
      var playerTopic =  WebSocketType.Player.ToString();
      consumer.Subscribe(new string[] { gameTopic, playerTopic });

      while (true)
      {
        var consumeResult = consumer.Consume();
        var message = JsonSerializer.Deserialize<OpenskullSocketKafkaMessage>(consumeResult.Message.Value);
        if (message is not null && consumeResult.Topic == gameTopic) {
          await _gameHubContext.Clients.Group(message.Id.ToString()).SendCoreAsync("send", new object[]{message.Message});
        }
        if (message is not null && consumeResult.Topic == playerTopic) {
          await _playerHubContext.Clients.Group(message.Id.ToString()).SendCoreAsync("send", new object[]{message.Message});
        }
      }
    }
  }
}