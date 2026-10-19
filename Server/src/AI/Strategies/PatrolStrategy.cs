namespace Bomberman.Server.src.Patterns.Strategy;

public sealed class PatrolStrategy : IMovementStrategy
{
    private int _direction = 1;
    public string Name => "Patrol horizontally";
    public GridPosition GetNextPosition(GridPosition c, GridPosition p, int minX, int minY, int maxX, int maxY)
    {
        int nextX = c.X + _direction;
        if (nextX > maxX || nextX < minX) { _direction *= -1; nextX = c.X + _direction; }
        return new(Math.Clamp(nextX, minX, maxX), Math.Clamp(c.Y, minY, maxY));
    }
}
