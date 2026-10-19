namespace Bomberman.Server.Blocks.Damage;

public interface IDamageSystem
{
    int ApplyDamage(int currentHealth, int incomingDamage);
}