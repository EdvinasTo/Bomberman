package bomberman.sound;

public interface IGameSound extends AutoCloseable {
    void play();

    @Override
    default void close() { }
}
