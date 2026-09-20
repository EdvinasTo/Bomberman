using System;
using System.Collections.Concurrent;
using Bomberman.Server.src.Models;
using Bomberman.Server.src.Interfaces;

namespace Bomberman.Server.src.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly ConcurrentDictionary<string, PlayerPosition> _positions = new();

        public PlayerPosition UpdatePosition(string connectionId, int x, int y)
        {
            int clampedX = Math.Clamp(x, 0, 610);
            int clampedY = Math.Clamp(y, 0, 450);
            var pos = new PlayerPosition(clampedX, clampedY);

            _positions[connectionId] = pos;
            return pos;
        }

        public PlayerPosition GetPosition(string connectionId)
        {
            return _positions.TryGetValue(connectionId, out var pos)
                ? pos
                : new PlayerPosition(100, 100);
        }

        public void RemovePlayer(string connectionId)
        {
            _positions.TryRemove(connectionId, out _);
        }
    }
}