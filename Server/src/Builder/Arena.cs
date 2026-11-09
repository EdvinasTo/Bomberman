using System.Collections.Concurrent;
using Bomberman.Server.Blocks;
using Bomberman.Server.src.Models;

namespace Bomberman.Server.src.Builder;

public sealed class Arena
{
    public const int TileSize = 60;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public PlayerPosition Player1Spawn { get; private set; } = new(80, 80);
    public PlayerPosition Player2Spawn { get; private set; } = new(550, 430);

    public ConcurrentDictionary<(int X, int Y), Block> Blocks { get; } = new();

    public void SetSize(int width, int height)
    {
        if (width < 5 || height < 5)
            throw new ArgumentOutOfRangeException(nameof(width), "Arena is too small.");
        Width = width;
        Height = height;
    }

    public void SetBlock(int x, int y, Block block)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (x % TileSize != 0 || y % TileSize != 0 ||
            x < 0 || y < 0 || x >= Width * TileSize || y >= Height * TileSize)
            throw new ArgumentOutOfRangeException(nameof(x), "Block must occupy a valid grid cell.");
        Blocks[(x, y)] = block;
    }

    public Block? GetBlock(int x, int y) =>
        Blocks.TryGetValue((x, y), out var block) ? block : null;

    public bool RemoveBlock(int x, int y) => Blocks.TryRemove((x, y), out _);

    public void SetPlayerSpawns(PlayerPosition player1, PlayerPosition player2)
    {
        ArgumentNullException.ThrowIfNull(player1);
        ArgumentNullException.ThrowIfNull(player2);
        if (!IsWalkable(player1.X, player1.Y) || !IsWalkable(player2.X, player2.Y))
            throw new InvalidOperationException("A player spawn overlaps a wall.");
        if (Math.Abs(player1.X - player2.X) < 30 && Math.Abs(player1.Y - player2.Y) < 30)
            throw new InvalidOperationException("Player spawn locations must not overlap.");
        Player1Spawn = player1;
        Player2Spawn = player2;
    }

    public bool IsWalkable(int x, int y, int entitySize = 30)
    {
        if (x < 0 || y < 0 || x + entitySize > Width * TileSize ||
            y + entitySize > Height * TileSize) return false;

        foreach (var pair in Blocks)
            if (x < pair.Key.X + TileSize && x + entitySize > pair.Key.X &&
                y < pair.Key.Y + TileSize && y + entitySize > pair.Key.Y)
                return false;

        return true;
    }

    // One character per cell, row-major; # = solid, + = destructible, . = floor.
    // SignalR transports the string as JSON, so both clients receive the exact same layout.
    public string ExportTiles()
    {
        var tiles = new char[Width * Height];
        Array.Fill(tiles, '.');
        foreach (var pair in Blocks)
        {
            int column = pair.Key.X / TileSize;
            int row = pair.Key.Y / TileSize;
            tiles[row * Width + column] = pair.Value.IsDestructible ? '+' : '#';
        }
        return new string(tiles);
    }
}
