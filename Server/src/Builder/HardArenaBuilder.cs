using Bomberman.Server.Blocks;
using Bomberman.Server.Blocks.Damage;
using Bomberman.Server.src.Models;

namespace Bomberman.Server.src.Builder;

public sealed class HardArenaBuilder : IArenaBuilder
{
    private Arena _arena = new();

    public void Reset() => _arena = new Arena();
    public void SetSize(int width, int height) => _arena.SetSize(width, height);

    public void AddOuterWalls()
    {
        for (int x = 0; x < _arena.Width; x++)
        {
            _arena.SetBlock(x * Arena.TileSize, 0, new IndestructibleBlock());
            _arena.SetBlock(x * Arena.TileSize, (_arena.Height - 1) * Arena.TileSize,
                new IndestructibleBlock());
        }
        for (int y = 1; y < _arena.Height - 1; y++)
        {
            _arena.SetBlock(0, y * Arena.TileSize, new IndestructibleBlock());
            _arena.SetBlock((_arena.Width - 1) * Arena.TileSize, y * Arena.TileSize,
                new IndestructibleBlock());
        }
    }

    public void AddInnerWalls()
    {
        foreach (var (x, y) in new[] {
            (120, 120), (240, 120), (360, 120), (480, 120),
            (120, 240), (240, 240), (360, 240), (480, 240),
            (120, 360), (240, 360), (360, 360), (480, 360)
        })
            _arena.SetBlock(x, y, new IndestructibleBlock());
    }

    public void AddDestructibleBlocks()
    {
        foreach (var (x, y) in new[] {
            (180, 60), (300, 60), (420, 60), (540, 60),
            (60, 180), (180, 180), (300, 180), (420, 180), (540, 180),
            (60, 300), (180, 300), (300, 300), (420, 300), (540, 300),
            (180, 420), (300, 420), (420, 420)
        })
            _arena.SetBlock(x, y, new DestructibleBlock(new OneHitDamage()));
    }

    public void SetPlayerSpawns() =>
        _arena.SetPlayerSpawns(new PlayerPosition(80, 80), new PlayerPosition(550, 430));

    public Arena GetResult() => _arena;
}
