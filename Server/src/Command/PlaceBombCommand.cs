using Bomberman.Server.src.Interfaces;
using Server.src.Facade;

namespace Bomberman.Server.src.Patterns.Command;

public class PlaceBombCommand : ICommand
{
    private readonly GameFacade _facade;
    private readonly string _playerId;

    public PlaceBombCommand(GameFacade facade, string playerId)
    {
        _facade = facade;
        _playerId = playerId;
    }

    // Fire and forget, like the old hub code: the bomb runs until it explodes.
    public void Execute() => _ = _facade.PlaceBomb(_playerId);

    public void Undo() => _facade.CancelBomb(_playerId);
}