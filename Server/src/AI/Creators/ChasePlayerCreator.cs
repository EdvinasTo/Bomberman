using Bomberman.Server.src.Patterns.Strategy;

namespace Bomberman.Server.src.Patterns.FactoryMethod;

public sealed class ChasePlayerCreator : MovementStrategyCreator
{
    public override IMovementStrategy CreateStrategy() => new ChasePlayerStrategy();
}
