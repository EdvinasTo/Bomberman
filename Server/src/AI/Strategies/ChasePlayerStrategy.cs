namespace Bomberman.Server.src.Patterns.Strategy;

public sealed class ChasePlayerStrategy : IMovementStrategy
{
    public string Name => "Chase player";
    public GridPosition GetNextPosition(GridPosition c, GridPosition p, int minX, int minY, int maxX, int maxY)
    {
        int dx = Math.Sign(p.X - c.X), dy = Math.Sign(p.Y - c.Y);
        if (Math.Abs(p.X - c.X) >= Math.Abs(p.Y - c.Y)) dy = 0; else dx = 0;
        const int speed = 4;
        return new(Math.Clamp(c.X + dx * speed, minX, maxX), Math.Clamp(c.Y + dy * speed, minY, maxY));
    }
}
