using Bomberman.Server.Blocks.Damage;
using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Models;
using Bomberman.Server.src.Patterns.FactoryMethod;
using Bomberman.Server.src.Patterns.Prototype;

namespace Bomberman.Server.src.Patterns.Strategy;

public sealed class Enemy : GameEntity, IEnemyPrototype
{
    private MovementStrategyCreator _movementCreator;
    private readonly Dictionary<MovementStrategyCreator, IMovementStrategy> _strategies = new();

    public GridPosition Position { get; private set; }

    public IMovementStrategy MovementStrategy { get; private set; }

    public Enemy(
        GridPosition start,
        MovementStrategyCreator movementCreator,
        IDamageSystem damageSystem)
        : base(2, damageSystem)
    {
        Position = start;

        _movementCreator = movementCreator
            ?? throw new ArgumentNullException(nameof(movementCreator));
        MovementStrategy = _movementCreator.CreateStrategy();
        _strategies.Add(_movementCreator, MovementStrategy);
    }

    private Enemy(Enemy source) : base(source)
    {
        Position = source.Position;
        // The creators are stateless; each enemy owns its strategy instances.
        _movementCreator = source._movementCreator;
        MovementStrategy = source.MovementStrategy.Clone();

        foreach (var pair in source._strategies)
        {
            var strategy = ReferenceEquals(pair.Value, source.MovementStrategy)
                ? MovementStrategy
                : pair.Value.Clone();
            _strategies.Add(pair.Key, strategy);
        }
    }

    public Enemy Clone() => new Enemy(this);

    // Copies value fields but shares strategies, their cache, and the damage system.
    public Enemy ShallowClone() => (Enemy)MemberwiseClone();

    public override bool IsAlive => Health > 0;

    public void SetMovementCreator(MovementStrategyCreator movementCreator)
    {
        _movementCreator = movementCreator
            ?? throw new ArgumentNullException(nameof(movementCreator));
    }

    public void UpdateBehavior()
    {
        // Keep strategy state per enemy when switching away and back.
        if (!_strategies.TryGetValue(_movementCreator, out var strategy))
        {
            strategy = _movementCreator.CreateStrategy();
            _strategies.Add(_movementCreator, strategy);
        }

        SetStrategy(strategy);
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
}
