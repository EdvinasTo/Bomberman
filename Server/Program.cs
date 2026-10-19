using Bomberman.Server.src.Controllers;
using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<IPlayerService, PlayerService>();
builder.Services.AddSingleton<IBombService, BombService>();
builder.Services.AddSingleton<EnemyService>();
builder.Services.AddHostedService<EnemyService>(sp => sp.GetRequiredService<EnemyService>());

var app = builder.Build();
app.MapHub<GameHub>("/game");
app.Run("http://localhost:5000");
