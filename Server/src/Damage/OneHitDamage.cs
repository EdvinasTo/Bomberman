namespace Bomberman.Server.Blocks.Damage;

public sealed class OneHitDamage : IDamageSystem
{
    public IDamageSystem Clone() => new OneHitDamage();

    public int ApplyDamage(int currentHealth, int incomingDamage)
    {
        return incomingDamage > 0 ? 0 : currentHealth;
    }
}
