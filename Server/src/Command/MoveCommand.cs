using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Models;

namespace Bomberman.Server.src.Patterns.Command;

public class MoveCommand : ICommand
{
    private readonly IPlayerService _players;
    private readonly string _playerId;
    private readonly int _targetX;
    private readonly int _targetY;
    private PlayerPosition _previous = default!;
    private bool _hasPrevious;

    public PlayerPosition Result { get; private set; } = default!;

    public MoveCommand(IPlayerService players, string playerId, int targetX, int targetY)
    {
        _players = players;
        _playerId = playerId;
        _targetX = targetX;
        _targetY = targetY;
    }

    public void Execute()
    {
        _previous = _players.GetPosition(_playerId);
        _hasPrevious = true;
        Result = _players.UpdatePosition(_playerId, _targetX, _targetY);
    }

    public void Undo()
    {
        if (!_hasPrevious) return;
        Result = _players.UpdatePosition(_playerId, _previous.X, _previous.Y);
    }
}