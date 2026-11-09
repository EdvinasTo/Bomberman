
package bomberman.model;

import java.awt.Point;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

public class GameState {
    private int playerX = 80;
    private int playerY = 80;
    private volatile int localPlayerSlot = 1;
    private volatile String arenaDifficulty = "Easy";
    private volatile int arenaWidth = 11;
    private volatile int arenaHeight = 9;
    private volatile int playerHealth = 3;

    private final Map<String, Point> otherPlayers =
            new ConcurrentHashMap<>();

    private final Map<String, Integer> otherPlayerSlots =
            new ConcurrentHashMap<>();

    private final List<Bomb> bombs =
            Collections.synchronizedList(new ArrayList<>());

    private final List<Explosion> explosions =
            Collections.synchronizedList(new ArrayList<>());

    private final Map<String, EnemyView> enemies =
            new ConcurrentHashMap<>();

    private final Map<String, BlockView> blocks =
            new ConcurrentHashMap<>();

    public GameState() {
        // Arena blocks come from the authoritative server, not duplicated here.
    }

    public synchronized void applyArenaSnapshot(
            String difficulty, int width, int height, String tiles) {
        if (width <= 0 || height <= 0 || tiles == null || tiles.length() != width * height) {
            throw new IllegalArgumentException("Invalid arena snapshot");
        }
        blocks.clear();
        for (int row = 0; row < height; row++) {
            for (int col = 0; col < width; col++) {
                char symbol = tiles.charAt(row * width + col);
                if (symbol == '#' || symbol == '+') {
                    int x = col * 60;
                    int y = row * 60;
                    blocks.put(key(x, y), new BlockView(x, y, symbol == '+'));
                } else if (symbol != '.') {
                    throw new IllegalArgumentException("Unexpected arena tile: " + symbol);
                }
            }
        }
        this.arenaWidth = width;
        this.arenaHeight = height;
        this.arenaDifficulty = difficulty;
    }

    public int getArenaWidth() { return arenaWidth; }
    public int getArenaHeight() { return arenaHeight; }
    public String getArenaDifficulty() { return arenaDifficulty; }
    public int getLocalPlayerSlot() { return localPlayerSlot; }
    public void setLocalPlayerSlot(int slot) { localPlayerSlot = slot; }
    public int getOtherPlayerSlot(String playerId) {
        return otherPlayerSlots.getOrDefault(playerId, 2);
    }

    private static String key(int x, int y) {
        return x + ":" + y;
    }

    public Map<String, BlockView> getBlocks() {
        return blocks;
    }

    public void removeBlock(int x, int y) {
        blocks.remove(key(x, y));
    }

    public synchronized int getPlayerX() {
        return playerX;
    }

    public synchronized int getPlayerY() {
        return playerY;
    }

    public int getPlayerHealth() {
        return playerHealth;
    }

    public void setPlayerHealth(int health) {
        playerHealth = Math.max(0, Math.min(3, health));
    }

    public synchronized void setPlayerPosition(int x, int y) {
        playerX = x;
        playerY = y;
    }

    // Remote player state.
    public Map<String, Point> getOtherPlayers() {
        return otherPlayers;
    }

    public void updateOtherPlayer(String id, int x, int y) {
        if (id == null || id.isBlank()) {
            return;
        }

        otherPlayers.put(id, new Point(x, y));
    }

    public void setOtherPlayerSlot(String id, int slot) {
        if (id != null) otherPlayerSlots.put(id, slot);
    }

    public void removeOtherPlayer(String id) {
        if (id != null) {
            otherPlayers.remove(id);
            otherPlayerSlots.remove(id);
        }
    }

    public List<Bomb> getBombs() {
        return bombs;
    }

    public void addBomb(int x, int y, int radius, String ownerId) {
        bombs.add(new Bomb(x, y, radius, ownerId));
    }

    public void removeBomb(int x, int y) {
        synchronized (bombs) {
            bombs.removeIf(bomb ->
                    bomb.isAt(x, y)
                    || bomb.isAt(x - 15, y - 15));
        }
    }

    public List<Explosion> getExplosions() {
        return explosions;
    }

    public void addExplosion(int x, int y, int radius) {
        // Render a cross using 30px cells.
        int step = 30;

        for (int offset = -radius; offset <= radius; offset += step) {
            explosions.add(new Explosion(x + offset, y));

            if (offset != 0) {
                explosions.add(new Explosion(x, y + offset));
            }
        }
    }

    public void removeExplosion(int x, int y, int radius) {
        synchronized (explosions) {
            explosions.removeIf(explosion ->
                    (explosion.getY() == y
                            && Math.abs(explosion.getX() - x) <= radius)
                    || (explosion.getX() == x
                            && Math.abs(explosion.getY() - y) <= radius));
        }
    }

    public Map<String, EnemyView> getEnemies() {
        return enemies;
    }

    public void updateEnemy(String id, int x, int y, int health) {
        if (id == null || id.isBlank()) {
            return;
        }

        if (health <= 0) {
            enemies.remove(id);
            return;
        }

        enemies.compute(id, (key, enemy) -> {
            if (enemy == null) {
                return new EnemyView(id, x, y, health);
            }

            enemy.setPosition(x, y);
            enemy.setHealth(health);
            return enemy;
        });
    }

    public void updateEnemyPosition(String id, int x, int y) {
        if (id == null || id.isBlank()) {
            return;
        }

        enemies.computeIfPresent(id, (key, enemy) -> {
            enemy.setPosition(x, y);
            return enemy;
        });
    }

    public void updateEnemyHealth(String id, int health) {
        if (id == null || id.isBlank()) {
            return;
        }

        enemies.computeIfPresent(id, (key, enemy) -> {
            enemy.setHealth(health);
            return health <= 0 ? null : enemy;
        });
    }

    public void removeEnemy(String id) {
        if (id != null) {
            enemies.remove(id);
        }
    }
}
