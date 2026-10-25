namespace Bomberman.Server.src.Patterns.Strategy;

public sealed class FleeFromPlayerStrategy : IMovementStrategy
{
    public string Name => "Flee from player";
    public IMovementStrategy Clone() => new FleeFromPlayerStrategy();
    public GridPosition GetNextPosition(GridPosition c, GridPosition p, int minX, int minY, int maxX, int maxY)
    {
        int dx = Math.Sign(c.X - p.X), dy = Math.Sign(c.Y - p.Y);
        if (Math.Abs(c.X - p.X) >= Math.Abs(c.Y - p.Y)) dy = 0; else dx = 0;
        return new(Math.Clamp(c.X + dx, minX, maxX), Math.Clamp(c.Y + dy, minY, maxY));
    }
}
