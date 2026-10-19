
using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Patterns.Singleton;
using Microsoft.AspNetCore.SignalR;

namespace Bomberman.Server.src.Controllers;

public sealed class GameHub : Hub
{
    private readonly IPlayerService _playerService;
    private readonly IBombService _bombService;
    private readonly GameManager _gameManager = GameManager.Instance;
    private readonly ILogger<GameHub> _logger;

    public GameHub(
        IPlayerService playerService,
        IBombService bombService,
        ILogger<GameHub> logger)
    {
        _playerService = playerService;
        _bombService = bombService;
        _logger = logger;
    }

    public async Task Move(int x, int y)
    {
        var position = _playerService.UpdatePosition(
            Context.ConnectionId, x, y);

        await Clients.Caller.SendAsync(
            "Position", position.X, position.Y);

        await Clients.Caller.SendAsync(
            "PlayerHealth",
            _playerService.GetPlayerHealth(Context.ConnectionId));
    }

    public Task PlaceBomb()
    {
        string connectionId = Context.ConnectionId;

        // Do not await the 3-second bomb countdown inside this hub invocation.
        _ = RunBombSafelyAsync(connectionId);

        return Task.CompletedTask;
    }

    private async Task RunBombSafelyAsync(string connectionId)
    {
        try
        {
            await _bombService.TryPlaceBombAsync(connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Failed to process bomb for connection {ConnectionId}",
                connectionId);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var position = _playerService.GetPosition(Context.ConnectionId);

        await Clients.Caller.SendAsync(
            "Position", position.X, position.Y);

        await Clients.Caller.SendAsync(
            "PlayerHealth",
            _playerService.GetPlayerHealth(Context.ConnectionId));

        foreach (var pair in _gameManager.GetEnemies())
        {
            await Clients.Caller.SendAsync(
                "EnemySpawned",
                pair.Key,
                pair.Value.Position.X,
                pair.Value.Position.Y,
                pair.Value.Health);
        }

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _playerService.RemovePlayer(Context.ConnectionId);
        _bombService.RemovePlayerBomb(Context.ConnectionId);

        return base.OnDisconnectedAsync(exception);
    }
}
