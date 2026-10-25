using Bomberman.Server.src.Patterns.Strategy;

namespace Bomberman.Server.src.Patterns.FactoryMethod;

public abstract class MovementStrategyCreator
{
    public abstract IMovementStrategy CreateStrategy();
}
