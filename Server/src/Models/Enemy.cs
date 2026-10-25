using Bomberman.Server.Blocks.Damage;
using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Models;

namespace Bomberman.Server.src.Patterns.Strategy;

public sealed class Enemy : GameEntity
{
    public GridPosition Position { get; private set; }

    public IMovementStrategy MovementStrategy { get; private set; }

    public Enemy(
        GridPosition start,
        IMovementStrategy movementStrategy,
        IDamageSystem damageSystem)
        : base(2, damageSystem)
    {
        Position = start;

        MovementStrategy = movementStrategy
            ?? throw new ArgumentNullException(nameof(movementStrategy));
    }

    public override bool IsAlive => Health > 0;

    public void SetStrategy(IMovementStrategy strategy)
    {
        MovementStrategy = strategy
            ?? throw new ArgumentNullException(nameof(strategy));
    }

    public GridPosition MoveToward(
        GridPosition player,
        int minX,
        int minY,
        int maxX,
        int maxY)
    {
        if (IsDead)
            return Position;

        Position = MovementStrategy.GetNextPosition(
            Position, player, minX, minY, maxX, maxY);

        return Position;
    }

    public void SetPosition(GridPosition position)
    {
        Position = position;
    }
}
