using System.Threading.Tasks;

namespace Bomberman.Server.src.Interfaces
{
    public interface IBombService
    {
        Task TryPlaceBombAsync(string connectionId);
        void RemovePlayerBomb(string connectionId);
    }
}