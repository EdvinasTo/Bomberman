package bomberman.observer;

import bomberman.sound.GameEvent;

public interface IGameSubject {
    void subscribe(IGameObserver o);
    void unsubscribe(IGameObserver o);
    void notifyObservers(GameEvent event);
}