using System.Collections.Concurrent;
using Bomberman.Server.src.Interfaces;

namespace Bomberman.Server.src.Patterns.Command;

public class CommandInvoker
{
    private readonly ConcurrentDictionary<string, Stack<ICommand>> _history = new();

    public void Run(string playerId, ICommand command)
    {
        command.Execute();

        var stack = _history.GetOrAdd(playerId, _ => new Stack<ICommand>());
        lock (stack) { stack.Push(command); }
    }

    public bool UndoLast(string playerId)
    {
        if (!_history.TryGetValue(playerId, out var stack)) return false;

        ICommand command;
        lock (stack)
        {
            if (stack.Count == 0) return false;
            command = stack.Pop();
        }

        command.Undo();
        return true;
    }

    public void Clear(string playerId) => _history.TryRemove(playerId, out _);
}