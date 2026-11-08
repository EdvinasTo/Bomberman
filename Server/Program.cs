using Bomberman.Server.src.Controllers;
using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Patterns.Observer;
using Bomberman.Server.src.Patterns.Singleton;
using Bomberman.Server.src.Services;
using Microsoft.AspNetCore.SignalR;
using Bomberman.Server.src.Patterns.Command;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<IPlayerService, PlayerService>();
builder.Services.AddSingleton<IBombService, BombService>();
builder.Services.AddSingleton<EnemyService>();
builder.Services.AddSingleton<CommandInvoker>();
builder.Services.AddHostedService<EnemyService>(sp => sp.GetRequiredService<EnemyService>());

var app = builder.Build();

GameManager.Instance.subscribe(
    new HubGameObserver(app.Services.GetRequiredService<IHubContext<GameHub>>()));

app.MapHub<GameHub>("/game");
app.Run("http://localhost:5000");
