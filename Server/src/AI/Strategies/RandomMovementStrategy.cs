namespace Bomberman.Server.src.Patterns.Strategy;

public sealed class RandomMovementStrategy : IMovementStrategy
{
    private readonly Random _random;
    public string Name => "Random movement";
    public RandomMovementStrategy() : this(Random.Shared) { }
    public RandomMovementStrategy(Random random) => _random = random;
    // A clone gets an independent random stream, rather than copying its sequence.
    public IMovementStrategy Clone() => new RandomMovementStrategy(new Random());
    public GridPosition GetNextPosition(GridPosition c, GridPosition p, int minX, int minY, int maxX, int maxY)
    {
        var direction = _random.Next(4);
        int x = c.X + (direction == 0 ? 1 : direction == 1 ? -1 : 0);
        int y = c.Y + (direction == 2 ? 1 : direction == 3 ? -1 : 0);
        return new(Math.Clamp(x, minX, maxX), Math.Clamp(y, minY, maxY));
    }
}
