namespace Bomberman.Server.Blocks;

public sealed class IndestructibleBlock : Block
{
    public IndestructibleBlock()
        : base(int.MaxValue, null)
    {
    }

    public override void TakeDamage(int damage)
    {
        // Indestructible blocks ignore all damage; no NoDamage class is needed.
    }
}
