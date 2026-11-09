namespace Bomberman.Server.src.Builder;

public sealed class ArenaDirector
{
    public Arena Construct(IArenaBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Reset();
        builder.SetSize(11, 9);
        builder.AddOuterWalls();
        builder.AddInnerWalls();
        builder.AddDestructibleBlocks();
        builder.SetPlayerSpawns();
        return builder.GetResult();
    }
}
