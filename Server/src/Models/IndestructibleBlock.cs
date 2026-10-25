namespace Bomberman.Server.Blocks;

public sealed class IndestructibleBlock : Block
{
    public override bool IsDestructible => false;

    public IndestructibleBlock()
        : base(int.MaxValue, null)
    {
    }

    public override void TakeDamage(int damage)
    {
        // This block cannot be damaged.
    }
}
