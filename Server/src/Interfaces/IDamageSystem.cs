namespace Bomberman.Server.Blocks.Damage;

public interface IDamageSystem
{
    IDamageSystem Clone();
    int ApplyDamage(int currentHealth, int incomingDamage);
}
