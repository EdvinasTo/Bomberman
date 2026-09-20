using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Bomberman.Server.src.Controllers;
using Bomberman.Server.src.Interfaces;
using System.Threading.Tasks;

namespace Bomberman.Server.src.Services
{
    public class BombService : IBombService
    {
        private readonly ConcurrentDictionary<string, bool> _activeBombs = new();
        private readonly IPlayerService _playerService;
        private readonly IHubContext<GameHub> _hubContext;

        public BombService(IPlayerService playerService, IHubContext<GameHub> hubContext)
        {
            _playerService = playerService;
            _hubContext = hubContext;
        }

        public async Task TryPlaceBombAsync(string connectionId)
        {
            // Prevent placing multiple bombs simultaneously
            if (_activeBombs.TryGetValue(connectionId, out bool active) && active)
            {
                return;
            }

            var position = _playerService.GetPosition(connectionId);
            _activeBombs[connectionId] = true;

            // 1. Broadcast immediately that bomb was placed
            await _hubContext.Clients.All.SendAsync("BombPlaced", position.X, position.Y);

            // 2. Offload lifecycle timer to background task
            _ = Task.Run(async () =>
            {
                try
                {
                    // 3 seconds fuse
                    await Task.Delay(3000);
                    await _hubContext.Clients.All.SendAsync("Exploded", position.X, position.Y);

                    // 0.5 seconds explosion duration
                    await Task.Delay(500);
                    _activeBombs[connectionId] = false;
                    await _hubContext.Clients.All.SendAsync("ExplosionFinished", position.X, position.Y);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during bomb timer execution: {ex.Message}");
                }
            });
        }

        public void RemovePlayerBomb(string connectionId)
        {
            _activeBombs.TryRemove(connectionId, out _);
        }
    }
}