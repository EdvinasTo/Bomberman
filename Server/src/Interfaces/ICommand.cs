namespace Bomberman.Server.src.Interfaces;

public interface ICommand
{
    void Execute();
    void Undo();
}

