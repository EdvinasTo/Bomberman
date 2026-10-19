using System.Collections.Concurrent;
using Bomberman.Server.src.Models;
using Bomberman.Server.src.Patterns.Strategy;
using Bomberman.Server.Blocks.Damage;

namespace Bomberman.Server.src.Patterns.Singleton;

/// <summary>
/// Thread-safe Singleton and authoritative in-memory game state.
/// The double-checked locking structure intentionally follows the requested pattern.
/// </summary>
public sealed class GameManager
{
    private static volatile GameManager? instance;
    private static readonly object syncRoot = new();

    private readonly ConcurrentDictionary<string, PlayerPosition> _playerPositions = new();
    private readonly ConcurrentDictionary<string, int> _playerHealth = new();
    private readonly ConcurrentDictionary<string, byte> _playersWithActiveBombs = new();
    private readonly ConcurrentDictionary<(int X, int Y), bool> _blocks = new();
    private readonly ConcurrentDictionary<string, Enemy> _enemies = new();
    private int _roundNumber;

    private const int PlayerSize = 30;
    private const int BlockSize = 60;
    private const int MaxHealth = 3;

    private GameManager()
    {
        // false = indestructible, true = destructible.
        for (int x = 0; x <= 600; x += BlockSize)
        {
            _blocks[(x, 0)] = false;
            _blocks[(x, 420)] = false;
        }

        for (int y = 60; y <= 360; y += BlockSize)
        {
            _blocks[(0, y)] = false;
            _blocks[(600, y)] = false;
        }

        foreach (var block in new[]
        {
            (180, 120), (240, 120), (360, 120),
            (420, 180), (180, 240), (300, 300), (420, 300)
        })
        {
            _blocks[block] = true;
        }
    }

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new GameManager();
                }
            }

            return instance;
        }
    }

    public int RoundNumber => Volatile.Read(ref _roundNumber);

    public int StartNextRound() => Interlocked.Increment(ref _roundNumber);

    public PlayerPosition UpdatePlayerPosition(string connectionId, int x, int y)
    {
        _playerHealth.TryAdd(connectionId, MaxHealth);
        var current = GetPlayerPosition(connectionId);

        x = Math.Clamp(x, 0, 610);
        y = Math.Clamp(y, 0, 450);

        // Check the full player rectangle, not only its top-left point.
        if (CanOccupy(x, y))
        {
            var updated = new PlayerPosition(x, y);
            _playerPositions[connectionId] = updated;
            return updated;
        }

        _playerPositions.TryAdd(connectionId, current);
        return _playerPositions[connectionId];
    }

    public PlayerPosition GetPlayerPosition(string connectionId)
    {
        if (_playerPositions.TryGetValue(connectionId, out var position))
            return position;

        _playerHealth.TryAdd(connectionId, MaxHealth);
        var initial = new PlayerPosition(100, 100);
        _playerPositions.TryAdd(connectionId, initial);
        return _playerPositions[connectionId];
    }

    public IReadOnlyCollection<KeyValuePair<string, PlayerPosition>> GetAllPlayerPositions() =>
        _playerPositions.ToArray();

    public int GetPlayerHealth(string connectionId) =>
        _playerHealth.GetOrAdd(connectionId, MaxHealth);

    public int ApplyExplosionDamage(string connectionId, int bombX, int bombY, int radius)
    {
        var position = GetPlayerPosition(connectionId);
        int centerX = position.X + PlayerSize / 2;
        int centerY = position.Y + PlayerSize / 2;

        bool hit =
            (Math.Abs(centerY - bombY) <= PlayerSize / 2 &&
             Math.Abs(centerX - bombX) <= radius + PlayerSize / 2) ||
            (Math.Abs(centerX - bombX) <= PlayerSize / 2 &&
             Math.Abs(centerY - bombY) <= radius + PlayerSize / 2);

        if (!hit)
            return GetPlayerHealth(connectionId);

        return ApplyPlayerDamage(connectionId, 1);
    }

    public int ApplyPlayerDamage(
    string connectionId,
    int damage,
    IDamageSystem? damageSystem = null)
    {
        if (damage <= 0)
            return GetPlayerHealth(connectionId);

        var system = damageSystem ?? new HealthBasedDamage();

        return _playerHealth.AddOrUpdate(
            connectionId,
            system.ApplyDamage(MaxHealth, damage),
            (_, health) => system.ApplyDamage(health, damage));
    }

    public bool TryStartPlayerBomb(string connectionId) =>
        _playersWithActiveBombs.TryAdd(connectionId, 0);

    public void FinishPlayerBomb(string connectionId) =>
        _playersWithActiveBombs.TryRemove(connectionId, out _);

    public bool CanOccupy(int x, int y)
    {
        foreach (var block in _blocks)
        {
            int bx = block.Key.X;
            int by = block.Key.Y;

            bool overlaps = x < bx + BlockSize && x + PlayerSize > bx &&
                            y < by + BlockSize && y + PlayerSize > by;
            if (overlaps)
                return false;
        }

        return true;
    }

    public IReadOnlyCollection<(int X, int Y)> DestroyBlocksInBlast(
        int bombX, int bombY, int radius)
    {
        var destroyed = new List<(int X, int Y)>();

        foreach (var block in _blocks)
        {
            if (!block.Value) // Indestructible.
                continue;

            int bx = block.Key.X;
            int by = block.Key.Y;
            int blockCenterX = bx + BlockSize / 2;
            int blockCenterY = by + BlockSize / 2;

            bool inHorizontalBlast =
                Math.Abs(blockCenterY - bombY) <= BlockSize / 2 &&
                Math.Abs(blockCenterX - bombX) <= radius + BlockSize / 2;
            bool inVerticalBlast =
                Math.Abs(blockCenterX - bombX) <= BlockSize / 2 &&
                Math.Abs(blockCenterY - bombY) <= radius + BlockSize / 2;

            if ((inHorizontalBlast || inVerticalBlast) &&
                _blocks.TryRemove(block.Key, out _))
            {
                destroyed.Add(block.Key);
            }
        }

        return destroyed;
    }

    public IReadOnlyCollection<KeyValuePair<string, Enemy>> GetEnemies() => _enemies.ToArray();

    public bool TryAddEnemy(string id, Enemy enemy) => _enemies.TryAdd(id, enemy);

    public bool TryGetEnemy(string id, out Enemy? enemy) => _enemies.TryGetValue(id, out enemy);

    public bool RemoveEnemy(string id) => _enemies.TryRemove(id, out _);

    public void RemovePlayer(string connectionId)
    {
        _playerPositions.TryRemove(connectionId, out _);
        _playerHealth.TryRemove(connectionId, out _);
        _playersWithActiveBombs.TryRemove(connectionId, out _);
    }
}
