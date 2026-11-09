
using System.Collections.Concurrent;
using Bomberman.Server.Blocks;
using Bomberman.Server.src.Builder;
using Bomberman.Server.Blocks.Damage;
using Bomberman.Server.src.Models;
using Bomberman.Server.src.Patterns.Strategy;
using Bomberman.Server.src.Patterns.Observer;

namespace Bomberman.Server.src.Patterns.Singleton;

public sealed class GameManager : IGameSubject
{
    private static volatile GameManager? instance;
    private static readonly object syncRoot = new();

    private readonly ConcurrentDictionary<string, Player> _players = new();
    private readonly ConcurrentDictionary<string, PlayerPosition> _playerPositions = new();
    private readonly ConcurrentDictionary<string, byte> _playersWithActiveBombs = new();
    private Arena _arena = null!;
    private ConcurrentDictionary<(int X, int Y), Block> _blocks = null!;
    private ArenaDifficulty _difficulty = ArenaDifficulty.Easy;
    private readonly ConcurrentDictionary<string, int> _playerSlots = new();
    private readonly ConcurrentDictionary<string, Enemy> _enemies = new();

    private readonly ConcurrentDictionary<IGameObserver, byte> _observers = new(); // observers

    private int _roundNumber;

    private const int PlayerSize = 30;
    private const int BlockSize = 60;
    private const int MaxHealth = 3;

    private GameManager()
    {
        ConfigureArena(ArenaDifficulty.Easy);
    }

    public ArenaDifficulty Difficulty => _difficulty;
    public int ArenaWidth => _arena.Width;
    public int ArenaHeight => _arena.Height;
    public PlayerPosition Player1Spawn => _arena.Player1Spawn;
    public PlayerPosition Player2Spawn => _arena.Player2Spawn;
    public string ArenaTiles => _arena.ExportTiles();

    // Called once at server startup, before clients join.
    public void ConfigureArena(ArenaDifficulty difficulty)
    {
        lock (syncRoot)
        {
            if (!_playerPositions.IsEmpty || !_playerSlots.IsEmpty)
                throw new InvalidOperationException("Cannot replace an arena while players are connected.");

            IArenaBuilder builder = difficulty == ArenaDifficulty.Hard
                ? new HardArenaBuilder()
                : new EasyArenaBuilder();
            _arena = new ArenaDirector().Construct(builder);
            _blocks = _arena.Blocks;
            _difficulty = difficulty;
        }
    }

    public bool TryReservePlayerSlot(string connectionId)
    {
        lock (syncRoot)
        {
            if (_playerSlots.ContainsKey(connectionId)) return true;
            if (_playerSlots.Count >= 2) return false;
            _playerSlots[connectionId] = _playerSlots.Values.Contains(1) ? 2 : 1;
            return true;
        }
    }

    public int GetPlayerSlot(string connectionId)
    {
        if (_playerSlots.TryGetValue(connectionId, out int slot))
            return slot;
        throw new InvalidOperationException("The player must reserve a slot before joining.");
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

        x = Math.Clamp(x, 0, _arena.Width * BlockSize - PlayerSize);
        y = Math.Clamp(y, 0, _arena.Height * BlockSize - PlayerSize);

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
            _ => GetPlayerSlot(connectionId) == 1
                ? _arena.Player1Spawn
                : _arena.Player2Spawn);
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

    public bool CanOccupy(int x, int y) =>
        _arena.IsWalkable(x, y, PlayerSize);

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
                notifyObservers(new GameEvent(GameEventType.BlockDestroyed, X: pair.Key.X, Y: pair.Key.Y));
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

    public bool RemoveEnemy(string id)
    {
        bool removed = _enemies.TryRemove(id, out _);
        if (removed) notifyObservers(new GameEvent(GameEventType.EnemyRemoved, id));
        return removed;
    }

    public void RemovePlayer(string connectionId)
    {
        _players.TryRemove(connectionId, out _);
        _playerSlots.TryRemove(connectionId, out _);
        _playerPositions.TryRemove(connectionId, out _);
        _playersWithActiveBombs.TryRemove(connectionId, out _);
        notifyObservers(new GameEvent(GameEventType.PlayerLeft, connectionId));
    }

    public void subscribe(IGameObserver o)   => _observers.TryAdd(o, 0);
    public void unsubscribe(IGameObserver o) => _observers.TryRemove(o, out _);

    public void notifyObservers(GameEvent e)
    {
        foreach (var o in _observers.Keys)
            o.OnGameEvent(e); // _ =
    }
}
