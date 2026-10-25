using Bomberman.Server.Blocks.Damage;

namespace Bomberman.Server.src.Models;

public abstract class GameEntity
{
    private readonly IDamageSystem _damageSystem;

    public int Health { get; protected set; }

    public bool IsDead => Health <= 0;

    public abstract bool IsAlive { get; }

    protected GameEntity(
        int initialHealth,
        IDamageSystem damageSystem)
    {
        if (initialHealth < 0)
            throw new ArgumentOutOfRangeException(nameof(initialHealth));

        Health = initialHealth;
        _damageSystem = damageSystem
            ?? throw new ArgumentNullException(nameof(damageSystem));
    }

    protected GameEntity(GameEntity source)
    {
        Health = source.Health;
        _damageSystem = source._damageSystem.Clone();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0)
            return;

        Health = _damageSystem.ApplyDamage(Health, damage);
    }
}
