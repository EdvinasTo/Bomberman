using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Models;
using Bomberman.Server.src.Patterns.Singleton;

namespace Bomberman.Server.src.Services;

public sealed class PlayerService : IPlayerService
{
    private readonly GameManager _gameManager = GameManager.Instance;

    public PlayerPosition UpdatePosition(
        string connectionId, int x, int y) =>
        _gameManager.UpdatePlayerPosition(connectionId, x, y);

    public PlayerPosition GetPosition(string connectionId) =>
        _gameManager.GetPlayerPosition(connectionId);

    public IReadOnlyCollection<KeyValuePair<string, PlayerPosition>>
        GetAllPositions() =>
        _gameManager.GetAllPlayerPositions();

    public int GetPlayerHealth(string connectionId) =>
        _gameManager.GetPlayerHealth(connectionId);

    public int ApplyExplosionDamage(
        string connectionId, int bombX, int bombY, int radius) =>
        _gameManager.ApplyExplosionDamage(
            connectionId, bombX, bombY, radius);

    public int ApplyDamage(string connectionId, int damage) =>
        _gameManager.ApplyPlayerDamage(connectionId, damage);

    public void RemovePlayer(string connectionId) =>
        _gameManager.RemovePlayer(connectionId);
}
