using Bomberman.Server.src.Controllers;
using Bomberman.Server.src.Services;
using Bomberman.Server.src.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Register SignalR
builder.Services.AddSignalR();

// Register application services as Singletons to preserve state
builder.Services.AddSingleton<IPlayerService, PlayerService>();
builder.Services.AddSingleton<IBombService, BombService>();

var app = builder.Build();

app.MapHub<GameHub>("/game");

app.Run("http://localhost:5000");