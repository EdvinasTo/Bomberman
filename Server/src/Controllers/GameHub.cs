using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Models;
using Bomberman.Server.src.Patterns.Command;
using Bomberman.Server.src.Patterns.Singleton;
using Microsoft.AspNetCore.SignalR;
using Server.src.Facade;

namespace Bomberman.Server.src.Controllers;

public sealed class GameHub : Hub
{
    private readonly GameFacade _facade;
    private readonly CommandInvoker _invoker;

    public GameHub(GameFacade facade, CommandInvoker invoker)
    {
        _facade = facade;
        _invoker = invoker;
    }

    public async Task Move(int x, int y)
    {
        var playerId = Context.ConnectionId;
        var command = new MoveCommand(_facade, playerId, x, y);
        _invoker.Run(playerId, command);
        await SendPositionAsync(playerId, command.Result);
    }

    public Task PlaceBomb()
    {
        var playerId = Context.ConnectionId;
        _invoker.Run(playerId, new PlaceBombCommand(_facade, playerId));
        return Task.CompletedTask;
    }

    public async Task Undo()
    {
        var playerId = Context.ConnectionId;
        if (_invoker.UndoLast(playerId))
            await SendPositionAsync(playerId, _facade.GetPosition(playerId));
    }

    private async Task SendPositionAsync(string playerId, PlayerPosition position)
    {
        await Clients.Caller.SendAsync("Position", position.X, position.Y);
        await Clients.Others.SendAsync("OtherPlayerPosition", playerId, position.X, position.Y);
        await Clients.Caller.SendAsync("PlayerHealth", _facade.GetHealth(playerId));
    }

    public override async Task OnConnectedAsync()
    {
        var playerId = Context.ConnectionId;
        var position = _facade.GetPosition(playerId);

        await Clients.Caller.SendAsync("Position", position.X, position.Y);

        await Clients.Caller.SendAsync("PlayerHealth", _facade.GetHealth(playerId));

        foreach (var pair in _facade.GetOtherPlayers(playerId))
            await Clients.Caller.SendAsync("OtherPlayerSpawned", pair.Key, pair.Value.X, pair.Value.Y);
        

        await Clients.Others.SendAsync(
            "OtherPlayerSpawned", playerId, position.X, position.Y);

        foreach (var enemy in _facade.GetEnemies())
        {
            await Clients.Caller.SendAsync(
                "EnemySpawned", enemy.Id, enemy.X, enemy.Y, enemy.Health);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var playerId = Context.ConnectionId;
        _invoker.Clear(playerId);

        await Clients.Others.SendAsync("PlayerLeft", playerId);

        _facade.LeavePlayer(playerId);
        _invoker.Clear(playerId);

        await base.OnDisconnectedAsync(exception);
    }
}
