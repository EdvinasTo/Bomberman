using Bomberman.Server.Blocks.Damage;

namespace Bomberman.Server.Blocks;

public abstract class Block
{
    private readonly IDamageSystem? _damageSystem;

    public int Health { get; protected set; }

    public bool IsDestroyed => Health <= 0;

    public abstract bool IsDestructible { get; }

    protected Block(int health, IDamageSystem? damageSystem)
    {
        Health = health;
        _damageSystem = damageSystem;
    }

    public virtual void TakeDamage(int damage)
    {
        if (_damageSystem == null || IsDestroyed || damage <= 0)
            return;

        Health = _damageSystem.ApplyDamage(Health, damage);
    }
}
