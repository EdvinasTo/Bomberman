package bomberman.sound;

import javax.sound.sampled.AudioInputStream;
import javax.sound.sampled.AudioSystem;
import javax.sound.sampled.Clip;
import javax.sound.sampled.LineUnavailableException;
import javax.sound.sampled.UnsupportedAudioFileException;
import java.io.BufferedInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.Objects;

public final class ClipSoundAdapter implements IGameSound {
    private final Clip clip;
    private boolean closed;

    public ClipSoundAdapter(Clip clip) {
        this.clip = Objects.requireNonNull(clip, "clip");
    }

    public static ClipSoundAdapter load(String resource)
            throws IOException, UnsupportedAudioFileException, LineUnavailableException {
        InputStream stream = ClipSoundAdapter.class.getResourceAsStream(resource);
        if (stream == null) {
            throw new IOException("Sound resource not found: " + resource);
        }

        try (InputStream buffered = new BufferedInputStream(stream);
             AudioInputStream audio = AudioSystem.getAudioInputStream(buffered)) {
            Clip clip = AudioSystem.getClip();
            try {
                clip.open(audio);
                return new ClipSoundAdapter(clip);
            } catch (IOException | LineUnavailableException | RuntimeException e) {
                clip.close();
                throw e;
            }
        }
    }

    @Override
    public synchronized void play() {
        if (closed) {
            return;
        }
        // Translate the game's play request into the Clip playback API.
        clip.stop();
        clip.setFramePosition(0);
        clip.start();
    }

    @Override
    public synchronized void close() {
        if (!closed) {
            closed = true;
            clip.close();
        }
    }
}
