using Bomberman.Server.src.Interfaces;
using Bomberman.Server.src.Models;
using Server.src.Facade;
using System.Runtime.InteropServices;

namespace Bomberman.Server.src.Patterns.Command;

public class MoveCommand : ICommand
{
    private readonly GameFacade _facade;
    private readonly string _playerId;
    private readonly int _targetX;
    private readonly int _targetY;
    private PlayerPosition _previous = default!;
    private bool _hasPrevious;

    public PlayerPosition Result { get; private set; } = default!;

    public MoveCommand(GameFacade facade, string playerId, int targetX, int targetY)
    {
        _facade = facade;
        _playerId = playerId;
        _targetX = targetX;
        _targetY = targetY;
    }

    public void Execute()
    {
        _previous = _facade.GetPosition(_playerId);
        _hasPrevious = true;
        Result = _facade.MovePlayer(_playerId, _targetX, _targetY);
    }

    public void Undo()
    {
        if (!_hasPrevious) return;
        Result = _facade.MovePlayer(_playerId, _previous.X, _previous.Y);
    }
}