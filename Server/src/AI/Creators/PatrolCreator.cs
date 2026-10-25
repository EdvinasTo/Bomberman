using Bomberman.Server.src.Patterns.Strategy;

namespace Bomberman.Server.src.Patterns.FactoryMethod;

public sealed class PatrolCreator : MovementStrategyCreator
{
    public override IMovementStrategy CreateStrategy() => new PatrolStrategy();
}
