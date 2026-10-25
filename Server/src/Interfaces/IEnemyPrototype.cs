using Bomberman.Server.src.Patterns.Strategy;

namespace Bomberman.Server.src.Patterns.Prototype;

public interface IEnemyPrototype
{
    Enemy Clone();
}
