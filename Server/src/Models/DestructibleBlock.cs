using Bomberman.Server.Blocks.Damage;

namespace Bomberman.Server.Blocks;

public sealed class DestructibleBlock : Block
{
    // The damage implementation can be replaced, while existing callers can
    // still construct a block with the default one-hit behavior.
    public DestructibleBlock(IDamageSystem? damageSystem = null)
        : base(1, damageSystem ?? new OneHitDamage())
    {
    }
}
