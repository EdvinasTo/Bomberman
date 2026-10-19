using Bomberman.Server.src.Controllers;
using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Patterns.Singleton;
using Bomberman.Server.src.Patterns.Strategy;
using Microsoft.AspNetCore.SignalR;
using Bomberman.Server.Blocks.Damage;

namespace Bomberman.Server.src.Services;

/// <summary>
/// Runs enemy AI. GameManager owns the authoritative enemy collection.
/// </summary>
public sealed class EnemyService : BackgroundService
{
    private readonly IHubContext<GameHub> _hubContext;
    private readonly IPlayerService _playerService;
    private readonly GameManager _gameManager = GameManager.Instance;

    private readonly Dictionary<string, PatrolStrategy> _patrol = new();
    private readonly Dictionary<string, RandomMovementStrategy> _random = new();

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

                        // Select the enemy's movement strategy.
                        if (enemy.Health <= 1)
                        {
                            enemy.SetStrategy(new FleeFromPlayerStrategy());
                        }
                        else if (distance <= 180)
                        {
                            enemy.SetStrategy(new ChasePlayerStrategy());
                        }
                        else if (distance <= 300)
                        {
                            enemy.SetStrategy(_random[pair.Key]);
                        }
                        else
                        {
                            enemy.SetStrategy(_patrol[pair.Key]);
                        }

                        Console.WriteLine(
                            $"Enemy {pair.Key} | HP: {enemy.Health} | " +
                            $"Strategy: {enemy.MovementStrategy.GetType().Name}");

                        // Save the position before moving.
                        var previousPosition = enemy.Position;

                        // Calculate the enemy's next position.
                        var position = enemy.MoveToward(
                            new GridPosition(
                                target.Value.X,
                                target.Value.Y),
                            0, 0, 610, 450);

                        // Reject movement if the destination overlaps a block.
                        if (!_gameManager.CanOccupy(
                                position.X,
                                position.Y))
                        {
                            enemy.SetPosition(previousPosition);
                            position = previousPosition;
                        }

                        // Send the authoritative enemy position to clients.
                        await _hubContext.Clients.All.SendAsync(
                            "EnemyPosition",
                            pair.Key,
                            position.X,
                            position.Y,
                            stoppingToken);

                        // Ghost contact instantly kills a player.
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

                            int health = _playerService.ApplyDamage(
                                player.Key,
                                1,
                                new OneHitDamage());

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

    private void SpawnEnemies()
    {
        for (int i = 0; i < SpawnPositions.Length; i++)
        {
            string id = Guid.NewGuid().ToString();

            var enemy = new Enemy(
                SpawnPositions[i],
                new PatrolStrategy());

            _gameManager.TryAddEnemy(id, enemy);

            _patrol[id] = new PatrolStrategy();
            _random[id] = new RandomMovementStrategy();

            _hubContext.Clients.All.SendAsync(
                "EnemySpawned",
                id,
                enemy.Position.X,
                enemy.Position.Y,
                2)
                .GetAwaiter()
                .GetResult();
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