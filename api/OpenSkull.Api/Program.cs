using OpenSkull.Api.Storage;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Queue;
using OpenSkull.Api.Messaging;
using OpenSkull.Api.Hubs;
using OpenSkull.Api.Middleware;
using Confluent.Kafka;
using StackExchange.Redis;
using System.Net;
using Cassandra;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GameCreateNew>(GameFunctions.CreateNew);
builder.Services.AddSingleton<TurnPlayCard>(GameFunctions.TurnPlayCard);
builder.Services.AddSingleton<TurnPlaceBid>(GameFunctions.TurnPlaceBid);
builder.Services.AddSingleton<TurnFlipCard>(GameFunctions.TurnFlipCard);


var kafkaString = System.Environment.GetEnvironmentVariable("KAFKA_CONNECTION_STRING");
if (kafkaString is not null)
{
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


var redisString = System.Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
if (redisString is not null)
{
  builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisString));
}

var gameCreationService = System.Environment.GetEnvironmentVariable("GAME_CREATION_SERVICE") ?? "MEMORY";

switch (gameCreationService)
{
  case "KAFKA":
    if (kafkaString is null)
    {
      throw new InvalidOperationException("Must provide a KAFKA_CONNECTION_STRING value");
    }
    builder.Services.AddSingleton<IGameCreationQueue, KafkaGameCreationQueue>();
    break;
  case "MEMORY":
  default:
    builder.Services.AddSingleton<IGameCreationQueue, GameCreationMemoryQueue>();
    break;
}

switch (System.Environment.GetEnvironmentVariable("WEBSOCKET_SERVICE") ?? "MEMORY")
{
  case "KAFKA":
    if (kafkaString is null)
    {
      throw new InvalidOperationException("Must provide a KAFKA_CONNECTION_STRING value");
    }
    builder.Services.AddSingleton<IWebSocketManager, KafkaWebSocketManager>();
    break;
  case "MEMORY":
  default:
    builder.Services.AddSingleton<IWebSocketManager, InMemoryWebSocketManager>();
    break;
}

switch (System.Environment.GetEnvironmentVariable("STORAGE_SERVICE") ?? "MEMORY")
{
  case "POSTGRES":
    var hostStrings = System.Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
    if (hostStrings is null)
    {
      throw new InvalidOperationException("Must provide a POSTGRES_CONNECTION_STRING value");
    }
    builder.Services.AddSingleton<IGameStorage>(new GamePostgresStorage(hostStrings));
    builder.Services.AddSingleton<IPlayerStorage>(new PlayerPostgresStorage(hostStrings));
    break;
  case "CASSANDRA":
    var contactPoints = System.Environment.GetEnvironmentVariable("CASSANDRA_CONTACT_POINTS");
    int port;
    var cassUsername = System.Environment.GetEnvironmentVariable("CASSANDRA_USERNAME");
    var cassPassword = System.Environment.GetEnvironmentVariable("CASSANDRA_PASSWORD");
    var cassKeyspace = System.Environment.GetEnvironmentVariable("CASSANDRA_KEYSPACE");
    if (!int.TryParse(System.Environment.GetEnvironmentVariable("CASSANDRA_PORT") ?? "9042", out port) || contactPoints is null || cassUsername is null || cassPassword is null || cassKeyspace is null)
    {
      throw new InvalidOperationException("Must provide all cassandra values");
    }
    var cluster = Cluster.Builder()
                        .AddContactPoints(contactPoints)
                        .WithPort(port)
                        .WithCredentials(cassUsername, cassPassword)
                        .Build();
    if (cluster is null) {
      throw new InvalidOperationException("Error setting up cassandra Cluster");
    }
    builder.Services.AddSingleton<IGameStorage>(new GameCassandraStorage(cluster, cassKeyspace));
    builder.Services.AddSingleton<IPlayerStorage>(new PlayerCassandraStorage(cluster, cassKeyspace));
              
    break;
  case "MEMORY":
  default:
    builder.Services.AddSingleton<IGameStorage, GameMemoryStorage>();
    builder.Services.AddSingleton<IPlayerStorage, PlayerMemoryStorage>();
    break;
}

switch (System.Environment.GetEnvironmentVariable("ROOM_STORAGE") ?? "MEMORY")
{
  case "REDIS":
    if (redisString is null)
    {
      throw new InvalidOperationException("Must provide a REDIS_CONNECTION_STRING value");
    }
    builder.Services.AddSingleton<IRoomStorage, RoomRedisStorage>();
    break;
  case "MEMORY":
  default:
    builder.Services.AddSingleton<IRoomStorage, RoomMemoryStorage>();
    break;
}
// TODO: Redis backing for this

builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

bool hasHost = System.Environment.GetEnvironmentVariable("OPENSKULL_WEBAPP_HOST") != null;

if (hasHost)
{
  builder.Services.AddCors(options =>
  {
    options.AddDefaultPolicy(
                      policy =>
                      {
                        policy.WithOrigins(System.Environment.GetEnvironmentVariable("OPENSKULL_WEBAPP_HOST")!)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                      });
  });
}

var app = builder.Build();

app.UseWebSockets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseVerifyPlayer();

if (hasHost)
{
  app.UseCors();
}

app.MapHub<PlayerHub>("/api/player/ws");
app.MapHub<GameHub>("/api/game/ws");

using (var serviceScope = app.Services.CreateScope())
{
  var services = serviceScope.ServiceProvider;

  var webSocketManager = services.GetRequiredService<IWebSocketManager>();
  Task.Run(webSocketManager.WebsocketMessageSenderThread);
  bool isGameMaster = false;
  if (gameCreationService == "MEMORY" || bool.TryParse(System.Environment.GetEnvironmentVariable("GAME_MASTER") ?? "false", out isGameMaster) && isGameMaster)
  {
    Console.WriteLine("Is GameMaster API");
    var gameCreationQueue = services.GetRequiredService<IGameCreationQueue>();
    Task.Run(gameCreationQueue.GameMasterThread);
  }
}

bool HIDE_CLIENT = false;
if (!bool.TryParse(System.Environment.GetEnvironmentVariable("HIDE_CLIENT"), out HIDE_CLIENT) || !HIDE_CLIENT)
{
  app.Use(async (context, next) =>
  {
    string path = context.Request.Path;
    if (path.StartsWith("/api"))
    {
      await next(context);
    }
    else
    {
      string directory = Environment.CurrentDirectory;
      string staticDirectory = "/Static";

      // This will probably fall over with binary data - but that can be future me's problem
      string fileToSend = File.Exists(directory + staticDirectory + path) ? directory + staticDirectory + path : directory + staticDirectory + "/index.html";
      var stream = File.OpenRead(fileToSend);
      string? contentType;
      new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider().TryGetContentType(fileToSend, out contentType);
      context.Response.ContentType = contentType ?? "text/html";
      await context.Response.WriteAsync(await new StreamReader(stream).ReadToEndAsync());
    }
  });
}

app.MapControllers();

app.Run();
