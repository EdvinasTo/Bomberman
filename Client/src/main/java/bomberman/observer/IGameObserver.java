package bomberman.observer;
import bomberman.sound.GameEvent;
public interface IGameObserver {
    void onGameEvent(GameEvent event);
}