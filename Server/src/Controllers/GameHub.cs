using Microsoft.AspNetCore.SignalR;
using Bomberman.Server.src.Interfaces;
using System.Threading.Tasks;
using System;

namespace Bomberman.Server.src.Controllers
{
    public class GameHub : Hub
    {
        private readonly IPlayerService _playerService;
        private readonly IBombService _bombService;

        public GameHub(IPlayerService playerService, IBombService bombService)
        {
            _playerService = playerService;
            _bombService = bombService;
        }

        public async Task Move(int x, int y)
        {
            var updatedPos = _playerService.UpdatePosition(Context.ConnectionId, x, y);
            await Clients.Caller.SendAsync("Position", updatedPos.X, updatedPos.Y);
        }

        public async Task PlaceBomb()
        {
            await _bombService.TryPlaceBombAsync(Context.ConnectionId);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _playerService.RemovePlayer(Context.ConnectionId);
            _bombService.RemovePlayerBomb(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}