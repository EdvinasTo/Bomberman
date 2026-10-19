
using Bomberman.Server.Blocks.Damage;
using Bomberman.Server.src.Interfaces;

namespace Bomberman.Server.src.Patterns.Strategy;

public sealed class Enemy
{
    public GridPosition Position { get; private set; }

    public IMovementStrategy MovementStrategy { get; private set; }

    public int Health { get; private set; } = 2;

    public bool IsDead => Health <= 0;

    private readonly IDamageSystem _damageSystem;

    public Enemy(
        GridPosition start,
        IMovementStrategy movementStrategy,
        IDamageSystem? damageSystem = null)
    {
        Position = start;
        MovementStrategy = movementStrategy
            ?? throw new ArgumentNullException(nameof(movementStrategy));

        _damageSystem = damageSystem ?? new HealthBasedDamage();
    }

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

    public void TakeDamage(int damage)
    {
        if (!IsDead && damage > 0)
            Health = _damageSystem.ApplyDamage(Health, damage);
    }
}
