using Bomberman.Server.src.Models;

namespace Bomberman.Server.src.Interfaces;

public interface IPlayerService
{
    PlayerPosition UpdatePosition(string connectionId, int x, int y);

    PlayerPosition GetPosition(string connectionId);

    IReadOnlyCollection<KeyValuePair<string, PlayerPosition>>
        GetAllPositions();

    int GetPlayerHealth(string connectionId);

    int ApplyExplosionDamage(
        string connectionId,
        int bombX,
        int bombY,
        int radius);

    int ApplyDamage(string connectionId, int damage);

    void RemovePlayer(string connectionId);
}
