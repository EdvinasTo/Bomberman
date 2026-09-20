using Bomberman.Server.src.Models;

namespace Bomberman.Server.src.Interfaces
{
    public interface IPlayerService
    {
        PlayerPosition UpdatePosition(string connectionId, int x, int y);
        PlayerPosition GetPosition(string connectionId);
        void RemovePlayer(string connectionId);
    }
}