using Bomberman.Server.Blocks.Damage;

namespace Bomberman.Server.Blocks;

// Bridge Abstraction: Player also delegates damage behavior to IDamageSystem.
// Player remains concrete because the game needs to instantiate players.
public sealed class Player
{
    private readonly IDamageSystem _damageSystem;

    public int Health { get; private set; } = 3;
    public bool IsDead => Health <= 0;

    public Player(IDamageSystem? damageSystem = null)
    {
        _damageSystem = damageSystem ?? new HealthBasedDamage();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0)
            return;

        Health = _damageSystem.ApplyDamage(Health, damage);
    }
}
