package bomberman.model;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public class GameState {
    private int playerX = 100;
    private int playerY = 100;

    private final List<Bomb> bombs = Collections.synchronizedList(new ArrayList<>());
    private final List<Explosion> explosions = Collections.synchronizedList(new ArrayList<>());

    // Player position
    public synchronized int getPlayerX() { return playerX; }
    public synchronized int getPlayerY() { return playerY; }
    public synchronized void setPlayerPosition(int x, int y) {
        this.playerX = x;
        this.playerY = y;
    }

    // Bomb management
    public List<Bomb> getBombs() { return bombs; }

    public void addBomb(int x, int y, int radius, String ownerId) {
        bombs.add(new Bomb(x, y, radius, ownerId));
    }

    public void removeBomb(int x, int y) {
        bombs.removeIf(b -> b.isAt(x, y));
    }

    // Explosion management
    public List<Explosion> getExplosions() { return explosions; }

    public void addExplosion(int x, int y) {
        // Cross pattern: center + 4 cardinal cells
        explosions.add(new Explosion(x, y));
        explosions.add(new Explosion(x + 60, y));
        explosions.add(new Explosion(x - 60, y));
        explosions.add(new Explosion(x, y + 60));
        explosions.add(new Explosion(x, y - 60));
    }

    public void removeExplosion(int x, int y) {
        explosions.removeIf(e -> Math.abs(e.getX() - x) <= 60 && Math.abs(e.getY() - y) <= 60);
    }
}