using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Models;
using Bomberman.Server.src.Patterns.Singleton;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;

namespace Server.src.Facade;

public class GameFacade
{
    private readonly IPlayerService _playerService;
    private readonly IBombService _bombService;
    private readonly GameManager _gameManager = GameManager.Instance;
    private readonly ILogger<GameFacade> _logger;

    public GameFacade(IPlayerService playerService, IBombService bombService, ILogger<GameFacade> logger)
    {
        _playerService = playerService;
        _bombService = bombService;
        _logger = logger;
    }

    public PlayerPosition MovePlayer(string id, int x, int y) =>
        _playerService.UpdatePosition(id, x, y);

    public PlayerPosition GetPosition(string id) =>
        _playerService.GetPosition(id);

    public int GetHealth(string id) =>
    _playerService.GetPlayerHealth(id);

    // bombs
    public Task PlaceBomb(string id)
    {
        try
        {
            _bombService.TryPlaceBombAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process bomb for connection {ConnectionId}",
                id);
        }
        return Task.CompletedTask;
    }

    public void CancelBomb(string id) =>
    _bombService.RemovePlayerBomb(id);

    // --- joining and leaving: combines several subsystems ---
    public PlayerPosition JoinPlayer(string id) =>
        _playerService.GetPosition(id);

    public IEnumerable<KeyValuePair<string, PlayerPosition>> GetOtherPlayers(string id) =>
        _playerService.GetAllPositions().Where(p => p.Key != id);

    public IEnumerable<(string Id, int X, int Y, int Health)> GetEnemies() =>
        _gameManager.GetEnemies().Select(p =>
            (p.Key, p.Value.Position.X, p.Value.Position.Y, p.Value.Health));

    public void LeavePlayer(string id)
    {
        _playerService.RemovePlayer(id);
        _bombService.RemovePlayerBomb(id);
    }
}

