using OpenSkull.Api.Storage;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Queue;
using OpenSkull.Api.Messaging;
using OpenSkull.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IGameStorage, GameMemoryStorage>();
builder.Services.AddSingleton<IGameCreationQueue, GameCreationMemoryQueue>();
builder.Services.AddSingleton<IWebSocketManager, InMemoryWebSocketManager>();
builder.Services.AddSingleton<GameCreateNew>(GameFunctions.CreateNew);
builder.Services.AddSingleton<TurnPlayCard>(GameFunctions.TurnPlayCard);
builder.Services.AddSingleton<TurnPlaceBid>(GameFunctions.TurnPlaceBid);
builder.Services.AddSingleton<TurnFlipCard>(GameFunctions.TurnFlipCard);

builder.Services.AddControllers();
builder.Services.AddSignalR();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var  MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy  =>
                      {
                          policy.WithOrigins("http://localhost:3000");
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

app.MapHub<PlayerHub>("/player/ws");
app.MapHub<GameHub>("/game/ws");

app.UseCors(MyAllowSpecificOrigins);

app.MapControllers();

app.Run();
