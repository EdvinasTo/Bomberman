
using System.Collections.Concurrent;
using Bomberman.Server.Blocks;
using Bomberman.Server.Blocks.Damage;
using Bomberman.Server.src.Models;
using Bomberman.Server.src.Patterns.Strategy;

namespace Bomberman.Server.src.Patterns.Singleton;

public sealed class GameManager
{
    private static volatile GameManager? instance;
    private static readonly object syncRoot = new();

    private readonly ConcurrentDictionary<string, Player> _players = new();
    private readonly ConcurrentDictionary<string, PlayerPosition> _playerPositions = new();
    private readonly ConcurrentDictionary<string, byte> _playersWithActiveBombs = new();
    private readonly ConcurrentDictionary<(int X, int Y), Block> _blocks = new();
    private readonly ConcurrentDictionary<string, Enemy> _enemies = new();

    private int _roundNumber;

    private const int PlayerSize = 30;
    private const int BlockSize = 60;
    private const int MaxHealth = 3;

    private GameManager()
    {
        // Indestructible border blocks.
        for (int x = 0; x <= 600; x += BlockSize)
        {
            _blocks[(x, 0)] = new IndestructibleBlock();
            _blocks[(x, 420)] = new IndestructibleBlock();
        }

        for (int y = 60; y <= 360; y += BlockSize)
        {
            _blocks[(0, y)] = new IndestructibleBlock();
            _blocks[(600, y)] = new IndestructibleBlock();
        }

        // Internal indestructible obstacle.
        _blocks[(300, 180)] = new IndestructibleBlock();

        // Destructible blocks.
        foreach (var position in new[]
        {
            (180, 120),
            (240, 120),
            (360, 120),
            (420, 180),
            (180, 240),
            (300, 300),
            (420, 300)
        })
        {
            _blocks[position] =
                new DestructibleBlock(new OneHitDamage());
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

    public int StartNextRound() =>
        Interlocked.Increment(ref _roundNumber);

    private Player GetOrCreatePlayer(string connectionId)
    {
        return _players.GetOrAdd(
            connectionId,
            _ => new Player(new HealthBasedDamage()));
    }

    public PlayerPosition UpdatePlayerPosition(
        string connectionId, int x, int y)
    {
        GetOrCreatePlayer(connectionId);

        var current = GetPlayerPosition(connectionId);

        x = Math.Clamp(x, 0, 610);
        y = Math.Clamp(y, 0, 450);

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
        GetOrCreatePlayer(connectionId);

        return _playerPositions.GetOrAdd(
            connectionId,
            _ => new PlayerPosition(100, 100));
    }

    public IReadOnlyCollection<KeyValuePair<string, PlayerPosition>>
        GetAllPlayerPositions()
    {
        // Ensure every registered position has a Player object.
        foreach (var pair in _playerPositions)
            GetOrCreatePlayer(pair.Key);

        return _playerPositions.ToArray();
    }

    public int GetPlayerHealth(string connectionId)
    {
        return GetOrCreatePlayer(connectionId).Health;
    }

    public int ApplyExplosionDamage(
        string connectionId, int bombX, int bombY, int radius)
    {
        var position = GetPlayerPosition(connectionId);

        int centerX = position.X + PlayerSize / 2;
        int centerY = position.Y + PlayerSize / 2;

        bool hit =
            (Math.Abs(centerY - bombY) <= PlayerSize / 2 &&
             Math.Abs(centerX - bombX) <= radius + PlayerSize / 2) ||
            (Math.Abs(centerX - bombX) <= PlayerSize / 2 &&
             Math.Abs(centerY - bombY) <= radius + PlayerSize / 2);

        if (hit)
            GetOrCreatePlayer(connectionId).TakeDamage(1);

        return GetPlayerHealth(connectionId);
    }

    public int ApplyPlayerDamage(string connectionId, int damage)
    {
        GetOrCreatePlayer(connectionId).TakeDamage(damage);
        return GetPlayerHealth(connectionId);
    }

    public bool TryStartPlayerBomb(string connectionId) =>
        _playersWithActiveBombs.TryAdd(connectionId, 0);

    public void FinishPlayerBomb(string connectionId) =>
        _playersWithActiveBombs.TryRemove(connectionId, out _);

    public bool CanOccupy(int x, int y)
    {
        foreach (var pair in _blocks)
        {
            int bx = pair.Key.X;
            int by = pair.Key.Y;

            bool overlaps =
                x < bx + BlockSize &&
                x + PlayerSize > bx &&
                y < by + BlockSize &&
                y + PlayerSize > by;

            if (overlaps)
                return false;
        }

        return true;
    }

    public IReadOnlyCollection<(int X, int Y)> DestroyBlocksInBlast(
        int bombX, int bombY, int radius)
    {
        var destroyed = new List<(int X, int Y)>();

        foreach (var pair in _blocks)
        {
            var block = pair.Value;

            if (!block.IsDestructible)
                continue;

            int bx = pair.Key.X;
            int by = pair.Key.Y;

            int centerX = bx + BlockSize / 2;
            int centerY = by + BlockSize / 2;

            bool horizontalHit =
                Math.Abs(centerY - bombY) <= BlockSize / 2 &&
                Math.Abs(centerX - bombX) <= radius + BlockSize / 2;

            bool verticalHit =
                Math.Abs(centerX - bombX) <= BlockSize / 2 &&
                Math.Abs(centerY - bombY) <= radius + BlockSize / 2;

            if (!horizontalHit && !verticalHit)
                continue;

            block.TakeDamage(1);

            if (block.IsDestroyed &&
                _blocks.TryRemove(pair.Key, out _))
            {
                destroyed.Add(pair.Key);
            }
        }

        return destroyed;
    }

    public IReadOnlyCollection<KeyValuePair<string, Enemy>> GetEnemies() =>
        _enemies.ToArray();

    public bool TryAddEnemy(string id, Enemy enemy) =>
        _enemies.TryAdd(id, enemy);

    public bool TryGetEnemy(string id, out Enemy? enemy) =>
        _enemies.TryGetValue(id, out enemy);

    public bool RemoveEnemy(string id) =>
        _enemies.TryRemove(id, out _);

    public void RemovePlayer(string connectionId)
    {
        _players.TryRemove(connectionId, out _);
        _playerPositions.TryRemove(connectionId, out _);
        _playersWithActiveBombs.TryRemove(connectionId, out _);
    }
}
