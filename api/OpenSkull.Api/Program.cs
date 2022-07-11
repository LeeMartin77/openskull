using OpenSkull.Api.Storage;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Queue;
using OpenSkull.Api.Messaging;
using OpenSkull.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<GameCreateNew>(GameFunctions.CreateNew);
builder.Services.AddSingleton<TurnPlayCard>(GameFunctions.TurnPlayCard);
builder.Services.AddSingleton<TurnPlaceBid>(GameFunctions.TurnPlaceBid);
builder.Services.AddSingleton<TurnFlipCard>(GameFunctions.TurnFlipCard);

builder.Services.AddSingleton<IWebSocketManager, InMemoryWebSocketManager>();

builder.Services.AddSingleton<IGameCreationQueue, GameCreationMemoryQueue>();

// Add services to the container.
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
    Task.Run(async () =>  await webSocketManager.WebsocketMessageSenderThread());
}

app.MapControllers();

app.Run();
