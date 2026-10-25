
using System.Collections.Concurrent;
using Bomberman.Server.src.Controllers;
using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Patterns.Singleton;
using Microsoft.AspNetCore.SignalR;

namespace Bomberman.Server.src.Services;

public sealed class BombService : IBombService
{
    private readonly IHubContext<GameHub> _hubContext;
    private readonly IPlayerService _playerService;
    private readonly GameManager _gameManager = GameManager.Instance;
    private readonly ConcurrentDictionary<string, byte> _runningBombs = new();

    private const int BombDelayMs = 3000;
    private const int ExplosionDurationMs = 500;
    private const int BlastRadius = 60;
    private const int EntitySize = 30;

    public BombService(
        IHubContext<GameHub> hubContext,
        IPlayerService playerService)
    {
        _hubContext = hubContext;
        _playerService = playerService;
    }

    public async Task TryPlaceBombAsync(string connectionId)
    {
        if (string.IsNullOrWhiteSpace(connectionId))
            return;

        if (!_gameManager.TryStartPlayerBomb(connectionId))
            return;

        if (!_runningBombs.TryAdd(connectionId, 0))
        {
            _gameManager.FinishPlayerBomb(connectionId);
            return;
        }

        try
        {
            var position = _playerService.GetPosition(connectionId);

            int bombX = position.X + EntitySize / 2;
            int bombY = position.Y + EntitySize / 2;

            await _hubContext.Clients.All.SendAsync(
                "BombPlaced", connectionId, position.X, position.Y);

            await Task.Delay(BombDelayMs);

            await _hubContext.Clients.All.SendAsync(
                "Exploded", connectionId, bombX, bombY, BlastRadius);

            foreach (var player in _playerService.GetAllPositions())
            {
                int health = _playerService.ApplyExplosionDamage(
                    player.Key, bombX, bombY, BlastRadius);

                await _hubContext.Clients.Client(player.Key)
                    .SendAsync("PlayerHealth", health);

                if (health <= 0)
                {
                    await _hubContext.Clients.Client(player.Key)
                        .SendAsync("PlayerDied");
                }
            }

            foreach (var pair in _gameManager.GetEnemies())
            {
                var enemy = pair.Value;

                if (!IsEnemyHit(
                        enemy.Position.X,
                        enemy.Position.Y,
                        bombX,
                        bombY,
                        BlastRadius))
                {
                    continue;
                }

                enemy.TakeDamage(1);

                if (enemy.IsDead)
                {
                    _gameManager.RemoveEnemy(pair.Key);

                    await _hubContext.Clients.All.SendAsync(
                        "EnemyHealth", pair.Key, 0);

                    await _hubContext.Clients.All.SendAsync(
                        "EnemyRemoved", pair.Key);
                }
                else
                {
                    await _hubContext.Clients.All.SendAsync(
                        "EnemyHealth", pair.Key, enemy.Health);
                }
            }

            foreach (var block in _gameManager.DestroyBlocksInBlast(
                         bombX, bombY, BlastRadius))
            {
                await _hubContext.Clients.All.SendAsync(
                    "BlockDestroyed", block.X, block.Y);
            }

            await Task.Delay(ExplosionDurationMs);

            await _hubContext.Clients.All.SendAsync(
                "ExplosionFinished", connectionId, bombX, bombY);
        }
        finally
        {
            _runningBombs.TryRemove(connectionId, out _);
            _gameManager.FinishPlayerBomb(connectionId);
        }
    }

    private static bool IsEnemyHit(
        int enemyX,
        int enemyY,
        int bombX,
        int bombY,
        int radius)
    {
        int enemyCenterX = enemyX + EntitySize / 2;
        int enemyCenterY = enemyY + EntitySize / 2;

        bool horizontalHit =
            Math.Abs(enemyCenterY - bombY) <= EntitySize / 2 &&
            Math.Abs(enemyCenterX - bombX) <= radius + EntitySize / 2;

        bool verticalHit =
            Math.Abs(enemyCenterX - bombX) <= EntitySize / 2 &&
            Math.Abs(enemyCenterY - bombY) <= radius + EntitySize / 2;

        return horizontalHit || verticalHit;
    }

    public void RemovePlayerBomb(string connectionId)
    {
        _runningBombs.TryRemove(connectionId, out _);
        _gameManager.FinishPlayerBomb(connectionId);
    }
}
