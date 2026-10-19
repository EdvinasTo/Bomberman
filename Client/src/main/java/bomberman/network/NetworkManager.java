
package bomberman.network;

import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import bomberman.model.GameState;

import javax.swing.*;

public class NetworkManager {
    private final HubConnection hub;
    private final GameState gameState;
    private final Runnable onStateUpdated;

    private volatile boolean ready = false;

    public NetworkManager(GameState gameState, Runnable onStateUpdated) {
        this.gameState = gameState;
        this.onStateUpdated = onStateUpdated;

        this.hub = HubConnectionBuilder
                .create("http://localhost:5000/game")
                .build();

        registerHandlers();
    }

    private void registerHandlers() {
        hub.on("Position", (Integer x, Integer y) -> {
            gameState.setPlayerPosition(x, y);
            notifyUI();
        }, Integer.class, Integer.class);

        hub.on("PlayerHealth", (Integer health) -> {
    gameState.setPlayerHealth(health);
    notifyUI();
}, Integer.class);

        hub.on("PlayerDied", () ->
                SwingUtilities.invokeLater(() ->
                        JOptionPane.showMessageDialog(
                                null,
                                "You died! Restart the game to play again."
                        )
                )
        );

        hub.on("BombPlaced", (String ownerId, Integer x, Integer y) -> {
            gameState.addBomb(x, y, 60, ownerId);
            notifyUI();
        }, String.class, Integer.class, Integer.class);

        hub.on("Exploded", (
                String ownerId, Integer x, Integer y, Integer radius) -> {
            gameState.removeBomb(x, y);
            gameState.addExplosion(x, y, radius);
            notifyUI();
        }, String.class, Integer.class, Integer.class, Integer.class);

        hub.on("ExplosionFinished", (
                String ownerId, Integer x, Integer y) -> {
            gameState.removeExplosion(x, y, 60);
            notifyUI();
        }, String.class, Integer.class, Integer.class);

        hub.on("EnemyPosition", (String id, Integer x, Integer y) -> {
            gameState.updateEnemyPosition(id, x, y);
            notifyUI();
        }, String.class, Integer.class, Integer.class);

        hub.on("EnemySpawned", (
                String id, Integer x, Integer y, Integer health) -> {
            gameState.updateEnemy(id, x, y, health);
            notifyUI();
        }, String.class, Integer.class, Integer.class, Integer.class);

        hub.on("EnemyHealth", (String id, Integer health) -> {
            gameState.updateEnemyHealth(id, health);
            notifyUI();
        }, String.class, Integer.class);

        hub.on("EnemyRemoved", (String id) -> {
            gameState.removeEnemy(id);
            notifyUI();
        }, String.class);

        hub.on("BlockDestroyed", (Integer x, Integer y) -> {
            gameState.removeBlock(x, y);
            notifyUI();
        }, Integer.class, Integer.class);

        hub.onClosed(error -> ready = false);
    }

    private void notifyUI() {
        if (onStateUpdated != null) {
            SwingUtilities.invokeLater(onStateUpdated);
        }
    }

    public void connect() {
        Thread connectionThread = new Thread(() -> {
            try {
                hub.start().blockingAwait();
                ready = true;

                sendMove(gameState.getPlayerX(), gameState.getPlayerY());

            } catch (Exception e) {
                e.printStackTrace();

                SwingUtilities.invokeLater(() ->
                        JOptionPane.showMessageDialog(
                                null,
                                "Failed to connect to the game server."
                        )
                );
            }
        }, "signalr-connect");

        connectionThread.setDaemon(true);
        connectionThread.start();
    }

    public void sendMove(int targetX, int targetY) {
        if (!ready) {
            return;
        }

        try {
            hub.send("Move", targetX, targetY);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void sendPlaceBomb() {
        if (!ready) {
            return;
        }

        try {
            hub.send("PlaceBomb");
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void disconnect() {
        ready = false;

        try {
            hub.stop().blockingAwait();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
