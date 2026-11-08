using Microsoft.AspNetCore.SignalR;
using Bomberman.Server.src.Patterns.Observer;
using Bomberman.Server.src.Controllers;

namespace Bomberman.Server.src.Patterns.Observer;

public class HubGameObserver : IGameObserver
{
    private readonly IHubContext<GameHub> _hub;
    public HubGameObserver(IHubContext<GameHub> hub) => _hub = hub;

    public Task OnGameEvent(GameEvent e)
    {
        if (e.Type == GameEventType.BlockDestroyed)
            return _hub.Clients.All.SendAsync("BlockDestroyed", e.X, e.Y);

        if (e.Type == GameEventType.EnemyRemoved)
            return _hub.Clients.All.SendAsync("EnemyRemoved", e.Id);

        if (e.Type == GameEventType.PlayerLeft)
            return _hub.Clients.All.SendAsync("PlayerLeft", e.Id);

        return Task.CompletedTask;
    }
}