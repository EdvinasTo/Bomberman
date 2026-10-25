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
        var playerId = Context.ConnectionId;

        var position = _playerService.UpdatePosition(playerId, x, y);

        await Clients.Caller.SendAsync(
            "Position", position.X, position.Y);

        await Clients.Others.SendAsync(
            "OtherPlayerPosition",
            playerId,
            position.X,
            position.Y);

        await Clients.Caller.SendAsync(
            "PlayerHealth",
            _playerService.GetPlayerHealth(playerId));
    }

    public Task PlaceBomb()
    {
        string connectionId = Context.ConnectionId;

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
                ex,
                "Failed to process bomb for connection {ConnectionId}",
                connectionId);
        }
    }

    public override async Task OnConnectedAsync()
    {
        var playerId = Context.ConnectionId;
        var position = _playerService.GetPosition(playerId);

        await Clients.Caller.SendAsync(
            "Position", position.X, position.Y);

        await Clients.Caller.SendAsync(
            "PlayerHealth",
            _playerService.GetPlayerHealth(playerId));

        foreach (var pair in _playerService.GetAllPositions())
        {
            if (pair.Key == playerId)
                continue;

            await Clients.Caller.SendAsync(
                "OtherPlayerSpawned",
                pair.Key,
                pair.Value.X,
                pair.Value.Y);
        }

        await Clients.Others.SendAsync(
            "OtherPlayerSpawned",
            playerId,
            position.X,
            position.Y);

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

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var playerId = Context.ConnectionId;

        await Clients.Others.SendAsync("PlayerLeft", playerId);

        _playerService.RemovePlayer(playerId);
        _bombService.RemovePlayerBomb(playerId);

        await base.OnDisconnectedAsync(exception);
    }
}
