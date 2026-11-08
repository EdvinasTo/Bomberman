namespace Bomberman.Server.src.Patterns.Observer;

public enum GameEventType { BlockDestroyed, EnemyRemoved, PlayerLeft }

public record GameEvent(GameEventType Type, string? Id = null, int X = 0, int Y = 0);

public interface IGameSubject {
    void subscribe(IGameObserver o);
    void unsubscribe(IGameObserver o);
    void notifyObservers(GameEvent e);
}

public interface IGameObserver
{
    Task OnGameEvent(GameEvent e);   // bomberman.sound.GameEvent
}