
using Bomberman.Server.Blocks.Damage;

namespace Bomberman.Server.src.Models;

public sealed class Player : GameEntity
{
    public Player(IDamageSystem damageSystem)
        : base(3, damageSystem)
    {
    }

    public override bool IsAlive => Health > 0;
}
