using Bomberman.Server.Blocks.Damage;

namespace Bomberman.Server.Blocks;

// Bridge Abstraction: a block delegates its damage behavior to an IDamageSystem.
public abstract class Block
{
    private readonly IDamageSystem? _damageSystem;

    public int Health { get; protected set; }
    public bool IsDestroyed => Health <= 0;

    protected Block(int health, IDamageSystem? damageSystem)
    {
        Health = health;
        _damageSystem = damageSystem;
    }

    public virtual void TakeDamage(int damage)
    {
        // A null damage system means this block ignores damage.
        if (_damageSystem == null || IsDestroyed || damage <= 0)
            return;

        Health = _damageSystem.ApplyDamage(Health, damage);
    }
}
