using Bomberman.Server.src.Controllers;
using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Patterns.Singleton;
using Bomberman.Server.src.Patterns.Strategy;
using Microsoft.AspNetCore.SignalR;
using Bomberman.Server.Blocks.Damage;
using Bomberman.Server.src.Patterns.FactoryMethod;
using Bomberman.Server.src.Patterns.Prototype;

namespace Bomberman.Server.src.Services;

public sealed class EnemyService : BackgroundService
{
    private readonly IHubContext<GameHub> _hubContext;
    private readonly IPlayerService _playerService;
    private readonly GameManager _gameManager = GameManager.Instance;

    private readonly MovementStrategyCreator _patrolCreator = new PatrolCreator();
    private readonly MovementStrategyCreator _randomCreator = new RandomMovementCreator();
    private readonly MovementStrategyCreator _chaseCreator = new ChasePlayerCreator();
    private readonly MovementStrategyCreator _fleeCreator = new FleeFromPlayerCreator();
    private readonly Dictionary<string, IEnemyPrototype> _prototypes = new();

    private static readonly GridPosition[] SpawnPositions =
    {
        new(500, 100),
        new(500, 350),
        new(100, 350)
    };

    public EnemyService(
        IHubContext<GameHub> hubContext,
        IPlayerService playerService)
    {
        _hubContext = hubContext;
        _playerService = playerService;
        RegisterPrototype("Patrol", new Enemy(
            new GridPosition(0, 0),
            _patrolCreator,
            new HealthBasedDamage()));
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        SpawnEnemies();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var players = _playerService.GetAllPositions();

                if (players.Count > 0)
                {
                    foreach (var pair in _gameManager.GetEnemies())
                    {
                        var enemy = pair.Value;

                        var target = players
                            .OrderBy(p => DistanceSquared(
                                enemy.Position.X,
                                enemy.Position.Y,
                                p.Value.X,
                                p.Value.Y))
                            .First();

                        double distance = Math.Sqrt(DistanceSquared(
                            enemy.Position.X,
                            enemy.Position.Y,
                            target.Value.X,
                            target.Value.Y));

                        if (enemy.Health <= 1)
                        {
                            enemy.SetMovementCreator(_fleeCreator);
                        }
                        else if (distance <= 180)
                        {
                            enemy.SetMovementCreator(_chaseCreator);
                        }
                        else if (distance <= 300)
                        {
                            enemy.SetMovementCreator(_randomCreator);
                        }
                        else
                        {
                            enemy.SetMovementCreator(_patrolCreator);
                        }

                        enemy.UpdateBehavior();

                        Console.WriteLine(
                            $"Enemy {pair.Key} | HP: {enemy.Health} | " +
                            $"Strategy: {enemy.MovementStrategy.GetType().Name}");

                        var previousPosition = enemy.Position;

                        var position = enemy.MoveToward(
                            new GridPosition(
                                target.Value.X,
                                target.Value.Y),
                            0, 0, 610, 450);

                        if (!_gameManager.CanOccupy(
                                position.X,
                                position.Y))
                        {
                            enemy.SetPosition(previousPosition);
                            position = previousPosition;
                        }

                        await _hubContext.Clients.All.SendAsync(
                            "EnemyPosition",
                            pair.Key,
                            position.X,
                            position.Y,
                            stoppingToken);

                        foreach (var player in players)
                        {
                            if (_gameManager.GetPlayerHealth(player.Key) <= 0)
                                continue;

                            if (!IsTouching(
                                    position.X,
                                    position.Y,
                                    player.Value.X,
                                    player.Value.Y))
                            {
                                continue;
                            }

                            int health = _playerService.ApplyDamage(player.Key, 1);

                            await _hubContext.Clients.Client(player.Key)
                                .SendAsync(
                                    "PlayerHealth",
                                    health,
                                    stoppingToken);

                            if (health <= 0)
                            {
                                await _hubContext.Clients.Client(player.Key)
                                    .SendAsync(
                                        "PlayerDied",
                                        stoppingToken);
                            }
                        }
                    }
                }

                await Task.Delay(100, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    public void RegisterPrototype(string name, IEnemyPrototype prototype)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(prototype);
        _prototypes[name] = prototype;
    }

    public Enemy SpawnEnemy(string name, int x, int y)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (!_prototypes.TryGetValue(name, out var prototype))
            throw new KeyNotFoundException($"Enemy prototype '{name}' is not registered.");

        var enemy = prototype.Clone();
        enemy.SetPosition(new GridPosition(x, y));
        string id = Guid.NewGuid().ToString();

        if (!_gameManager.TryAddEnemy(id, enemy))
            throw new InvalidOperationException($"Could not add enemy '{id}'.");

        _hubContext.Clients.All.SendAsync(
            "EnemySpawned",
            id,
            enemy.Position.X,
            enemy.Position.Y,
            enemy.Health)
            .GetAwaiter()
            .GetResult();

        return enemy;
    }

    public void SpawnEnemies()
    {
        foreach (var position in SpawnPositions)
        {
            SpawnEnemy("Patrol", position.X, position.Y);
        }
    }

    private static double DistanceSquared(
        int x1, int y1,
        int x2, int y2)
    {
        double dx = x1 - x2;
        double dy = y1 - y2;

        return dx * dx + dy * dy;
    }

    private static bool IsTouching(
        int enemyX, int enemyY,
        int playerX, int playerY)
    {
        const int entitySize = 30;

        return enemyX < playerX + entitySize
            && enemyX + entitySize > playerX
            && enemyY < playerY + entitySize
            && enemyY + entitySize > playerY;
    }
}
