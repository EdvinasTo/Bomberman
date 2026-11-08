using Microsoft.AspNetCore.SignalR;
using Bomberman.Server.src.Patterns.Observer;
using Bomberman.Server.src.Controllers;

namespace Bomberman.Server.src.Patterns.Observer;

public class HubGameObserver : IGameObserver
{
    private readonly IHubContext<GameHub> _hub;
    public HubGameObserver(IHubContext<GameHub> hub) => _hub = hub;

    public Task OnGameEvent(GameEvent e) => e.Type switch
    {
        GameEventType.BlockDestroyed => _hub.Clients.All.SendAsync("BlockDestroyed", e.X, e.Y),
        GameEventType.EnemyRemoved => _hub.Clients.All.SendAsync("EnemyRemoved", e.Id),
        GameEventType.PlayerLeft => _hub.Clients.All.SendAsync("PlayerLeft", e.Id),
        _ => Task.CompletedTask
    };
}