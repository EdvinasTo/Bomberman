namespace Bomberman.Server.src.Patterns.Strategy;

public readonly record struct GridPosition(int X, int Y);

public interface IMovementStrategy
{
    string Name { get; }
    GridPosition GetNextPosition(GridPosition current, GridPosition player, int minX, int minY, int maxX, int maxY);
}
