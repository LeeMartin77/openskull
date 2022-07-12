using OpenSkull.Api.Storage;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Queue;
using OpenSkull.Api.Messaging;
using OpenSkull.Api.Hubs;
using Confluent.Kafka;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GameCreateNew>(GameFunctions.CreateNew);
builder.Services.AddSingleton<TurnPlayCard>(GameFunctions.TurnPlayCard);
builder.Services.AddSingleton<TurnPlaceBid>(GameFunctions.TurnPlaceBid);
builder.Services.AddSingleton<TurnFlipCard>(GameFunctions.TurnFlipCard);


builder.Services.AddSingleton<IGameCreationQueue, GameCreationMemoryQueue>();

switch (System.Environment.GetEnvironmentVariable("QUEUE_SERVICE") ?? "MEMORY") {
    case "KAFKA":
        var kafkaString = System.Environment.GetEnvironmentVariable("KAFKA_CONNECTION_STRING");
        if (kafkaString is null) {
            throw new InvalidOperationException("Must provide a KAFKA_CONNECTION_STRING value");
        }
        builder.Services.AddSingleton<ProducerConfig>(new ProducerConfig
            {
                BootstrapServers = kafkaString,
                ClientId = Dns.GetHostName(),
            });
        builder.Services.AddSingleton<ConsumerConfig>(new ConsumerConfig
            {
                BootstrapServers = kafkaString,
                GroupId = Dns.GetHostName(),
                AutoOffsetReset = AutoOffsetReset.Earliest,
                AllowAutoCreateTopics = true
            });
        builder.Services.AddSingleton<IWebSocketManager, KafkaWebSocketManager>();
        break;
    case "MEMORY":
    default:
        builder.Services.AddSingleton<IWebSocketManager, InMemoryWebSocketManager>();
        break;
}

switch (System.Environment.GetEnvironmentVariable("STORAGE_SERVICE") ?? "MEMORY") {
    case "POSTGRES":
        var hostStrings = System.Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        if (hostStrings is null) {
            throw new InvalidOperationException("Must provide a POSTGRES_CONNECTION_STRING value");
        }
        builder.Services.AddSingleton<IGameStorage>(new GamePostgresStorage(hostStrings));
        break;
    case "MEMORY":
    default:
        builder.Services.AddSingleton<IGameStorage, GameMemoryStorage>();
        break;
}

builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                      policy  =>
                      {
                          policy.WithOrigins(System.Environment.GetEnvironmentVariable("OPENSKULL_WEBAPP_HOST") ?? "https://play.openskull.dev")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                      });
});

var app = builder.Build();

app.UseWebSockets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment()) {
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.UseCors();

app.MapHub<PlayerHub>("/player/ws");
app.MapHub<GameHub>("/game/ws");

using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;

    var webSocketManager = services.GetRequiredService<IWebSocketManager>();
    Task.Run(webSocketManager.WebsocketMessageSenderThread);
}

app.MapControllers();

app.Run();
