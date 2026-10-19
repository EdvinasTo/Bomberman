namespace Bomberman.Server.Blocks.Damage;

public sealed class HealthBasedDamage : IDamageSystem
{
    public int ApplyDamage(int currentHealth, int incomingDamage)
    {
        return Math.Max(0, currentHealth - Math.Max(0, incomingDamage));
    }
}