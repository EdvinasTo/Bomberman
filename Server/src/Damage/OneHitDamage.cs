namespace Bomberman.Server.Blocks.Damage;

public sealed class OneHitDamage : IDamageSystem
{
    public int ApplyDamage(int currentHealth, int incomingDamage)
    {
        return incomingDamage > 0 ? 0 : currentHealth;
    }
}