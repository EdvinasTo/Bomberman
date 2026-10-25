using Bomberman.Server.Blocks.Damage;

namespace Bomberman.Server.Blocks;

public sealed class DestructibleBlock : Block
{
    public override bool IsDestructible => true;

    public DestructibleBlock(IDamageSystem damageSystem)
        : base(1, damageSystem)
    {
    }
}
