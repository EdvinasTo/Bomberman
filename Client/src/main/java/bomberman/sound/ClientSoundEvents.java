package bomberman.sound;

import javax.sound.sampled.LineUnavailableException;
import javax.sound.sampled.UnsupportedAudioFileException;
import java.io.IOException;
import java.util.EnumMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Objects;

public final class ClientSoundEvents implements AutoCloseable {
    private final Map<GameEvent, IGameSound> sounds = new EnumMap<>(GameEvent.class);
    private boolean closed;

    public synchronized void register(GameEvent event, IGameSound sound) {
        Objects.requireNonNull(event, "event");
        Objects.requireNonNull(sound, "sound");
        if (closed) {
            throw new IllegalStateException("Sound events are closed.");
        }
        IGameSound previous = sounds.put(event, sound);
        if (previous != null && previous != sound && !sounds.containsValue(previous)) {
            previous.close();
        }
    }

    public synchronized void onGameEvent(GameEvent event) {
        IGameSound sound = sounds.get(Objects.requireNonNull(event, "event"));
        if (!closed && sound != null) {
            sound.play();
        }
    }

    public static ClientSoundEvents createDefault() {
        ClientSoundEvents events = new ClientSoundEvents();
        events.registerResource(GameEvent.BOMB_PLACED, "/sounds/bomb-placed.wav");
        events.registerResource(GameEvent.EXPLOSION, "/sounds/explosion.wav");
        events.registerResource(GameEvent.PLAYER_DIED, "/sounds/player-died.wav");
        return events;
    }

    private void registerResource(GameEvent event, String resource) {
        try {
            register(event, ClipSoundAdapter.load(resource));
        } catch (IOException | UnsupportedAudioFileException | LineUnavailableException
                | IllegalArgumentException | SecurityException e) {
            System.err.println("Sound disabled for " + event + ": " + e.getMessage());
        }
    }

    @Override
    public synchronized void close() {
        if (!closed) {
            closed = true;
            new HashSet<>(sounds.values()).forEach(IGameSound::close);
            sounds.clear();
        }
    }
}
