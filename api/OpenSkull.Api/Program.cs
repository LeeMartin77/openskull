using OpenSkull.Api.Storage;
using OpenSkull.Api.Functions;
using OpenSkull.Api.Queue;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IGameStorage, GameMemoryStorage>();
builder.Services.AddSingleton<IGameCreationQueue, GameCreationMemoryQueue>();
builder.Services.AddSingleton<GameCreateNew>(GameFunctions.CreateNew);
builder.Services.AddSingleton<TurnPlayCard>(GameFunctions.TurnPlayCard);
builder.Services.AddSingleton<TurnPlaceBid>(GameFunctions.TurnPlaceBid);
builder.Services.AddSingleton<TurnFlipCard>(GameFunctions.TurnFlipCard);

builder.Services.AddControllers();
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(MyAllowSpecificOrigins);

app.MapControllers();

app.Run();
