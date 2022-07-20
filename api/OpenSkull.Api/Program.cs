using OpenSkull.Api.Storage;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Queue;
using OpenSkull.Api.Messaging;
using OpenSkull.Api.Hubs;
using OpenSkull.Api.Middleware;
using Confluent.Kafka;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GameCreateNew>(GameFunctions.CreateNew);
builder.Services.AddSingleton<TurnPlayCard>(GameFunctions.TurnPlayCard);
builder.Services.AddSingleton<TurnPlaceBid>(GameFunctions.TurnPlaceBid);
builder.Services.AddSingleton<TurnFlipCard>(GameFunctions.TurnFlipCard);


var kafkaString = System.Environment.GetEnvironmentVariable("KAFKA_CONNECTION_STRING");
if (kafkaString is not null) {
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
}

var gameCreationService = System.Environment.GetEnvironmentVariable("GAME_CREATION_SERVICE") ?? "MEMORY";

switch (gameCreationService) {
    case "KAFKA":
        if (kafkaString is null) {
            throw new InvalidOperationException("Must provide a KAFKA_CONNECTION_STRING value");
        }
        builder.Services.AddSingleton<IGameCreationQueue, KafkaGameCreationQueue>();
        break;
    case "MEMORY":
    default:
        builder.Services.AddSingleton<IGameCreationQueue, GameCreationMemoryQueue>();
        break;
}

switch (System.Environment.GetEnvironmentVariable("WEBSOCKET_SERVICE") ?? "MEMORY") {
    case "KAFKA":
        if (kafkaString is null) {
            throw new InvalidOperationException("Must provide a KAFKA_CONNECTION_STRING value");
        }
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
        builder.Services.AddSingleton<IPlayerStorage>(new PlayerPostgresStorage(hostStrings));
        break;
    case "MEMORY":
    default:
        builder.Services.AddSingleton<IGameStorage, GameMemoryStorage>();
        builder.Services.AddSingleton<IPlayerStorage, PlayerMemoryStorage>();
        break;
}

// TODO: Redis backing for this
builder.Services.AddSingleton<IRoomStorage, RoomMemoryStorage>();

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
                          policy.WithOrigins(System.Environment.GetEnvironmentVariable("OPENSKULL_WEBAPP_HOST") ?? "http://localhost:3000")
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

app.UseVerifyPlayer();

app.UseCors();

app.MapHub<PlayerHub>("/player/ws");
app.MapHub<GameHub>("/game/ws");

using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;

    var webSocketManager = services.GetRequiredService<IWebSocketManager>();
    Task.Run(webSocketManager.WebsocketMessageSenderThread);
    bool isGameMaster = false;
    if(gameCreationService == "MEMORY" || bool.TryParse(System.Environment.GetEnvironmentVariable("GAME_MASTER") ?? "false", out isGameMaster) && isGameMaster) {
        Console.WriteLine("Is GameMaster API");
        var gameCreationQueue = services.GetRequiredService<IGameCreationQueue>();
        Task.Run(gameCreationQueue.GameMasterThread);
    }
}

app.MapControllers();

app.Run();
