using Bomberman.Server.src.Controllers;
using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Patterns.Command;
using Bomberman.Server.src.Patterns.Observer;
using Bomberman.Server.src.Patterns.Singleton;
using Bomberman.Server.src.Services;
using Microsoft.AspNetCore.SignalR;
using Server.src.Facade;
using Bomberman.Server.src.Builder;

// GoF Builder demonstration without starting the network server.
if (args.Contains("--demo-builder", StringComparer.OrdinalIgnoreCase))
{
    var director = new ArenaDirector();
    int previousDestructible = -1;
    foreach (var (name, arenaBuilder) in new (string Name, IArenaBuilder Builder)[]
    {
        ("Easy", new EasyArenaBuilder()),
        ("Hard", new HardArenaBuilder())
    })
    {
        var arena = director.Construct(arenaBuilder);
        Console.WriteLine($"{name}: {arena.Width}x{arena.Height}, " +
            $"destructible blocks = {arena.Blocks.Values.Count(b => b.IsDestructible)}");
        if (arena.Width != 11 || arena.Height != 9 || tilesAreInvalid(arena))
            throw new InvalidOperationException($"Invalid {name} arena construction.");
        int destructible = arena.Blocks.Values.Count(b => b.IsDestructible);
        if (name == "Hard" && destructible <= previousDestructible)
            throw new InvalidOperationException("Hard arena must contain more destructible blocks than Easy.");
        previousDestructible = destructible;
        var tiles = arena.ExportTiles();
        for (int row = 0; row < arena.Height; row++)
            Console.WriteLine(tiles.Substring(row * arena.Width, arena.Width));
        Console.WriteLine();
    }
    Console.WriteLine("Builder demonstration passed: two valid, distinct arenas.");
    return;
}

static bool tilesAreInvalid(Arena arena) =>
    arena.ExportTiles().Length != arena.Width * arena.Height ||
    !arena.IsWalkable(arena.Player1Spawn.X, arena.Player1Spawn.Y) ||
    !arena.IsWalkable(arena.Player2Spawn.X, arena.Player2Spawn.Y);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<IPlayerService, PlayerService>();
builder.Services.AddSingleton<IBombService, BombService>();
builder.Services.AddSingleton<EnemyService>();
builder.Services.AddSingleton<CommandInvoker>();
builder.Services.AddSingleton<GameFacade>();
builder.Services.AddHostedService<EnemyService>(sp => sp.GetRequiredService<EnemyService>());

var app = builder.Build();

// Configure the server-authoritative arena before the multiplayer session begins.
var difficultyText = app.Configuration["Arena:Difficulty"] ?? "Easy";
if (!Enum.TryParse<ArenaDifficulty>(difficultyText, true, out var difficulty))
    throw new ArgumentException($"Unsupported Arena:Difficulty '{difficultyText}'. Use Easy or Hard.");
GameManager.Instance.ConfigureArena(difficulty);
Console.WriteLine($"Bomberman arena: {difficulty} (11 x 9)");

GameManager.Instance.subscribe(
    new HubGameObserver(app.Services.GetRequiredService<IHubContext<GameHub>>()));

app.MapHub<GameHub>("/game");
app.Run("http://localhost:5000");
