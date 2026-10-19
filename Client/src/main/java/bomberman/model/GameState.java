
package bomberman.model;

import java.awt.Point;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

public class GameState {
    private int playerX = 100;
    private int playerY = 100;
    private volatile int playerHealth = 3;

    private final Map<String, Point> otherPlayers =
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
        // Indestructible border blocks.
        for (int x = 0; x <= 600; x += 60) {
            blocks.put(key(x, 0), new BlockView(x, 0, false));
            blocks.put(key(x, 420), new BlockView(x, 420, false));
        }

        for (int y = 60; y <= 360; y += 60) {
            blocks.put(key(0, y), new BlockView(0, y, false));
            blocks.put(key(600, y), new BlockView(600, y, false));
        }

        // Internal indestructible obstacle.
        blocks.put(
                key(300, 180),
                new BlockView(300, 180, false)
        );

        // Destructible blocks.
        int[][] positions = {
                {180, 120},
                {240, 120},
                {360, 120},
                {420, 180},
                {180, 240},
                {300, 300},
                {420, 300}
        };

        for (int[] position : positions) {
            blocks.put(
                    key(position[0], position[1]),
                    new BlockView(position[0], position[1], true)
            );
        }
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

    public void removeOtherPlayer(String id) {
        if (id != null) {
            otherPlayers.remove(id);
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
