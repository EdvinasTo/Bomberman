package bomberman.network;

import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import bomberman.model.GameState;

import javax.swing.*;

public class NetworkManager {
    private final HubConnection hub;
    private final GameState gameState;
    private final Runnable onStateUpdated;

    private boolean ready = false;
    private boolean waiting = false;

    public NetworkManager(GameState gameState, Runnable onStateUpdated) {
        this.gameState = gameState;
        this.onStateUpdated = onStateUpdated;
        this.hub = HubConnectionBuilder.create("http://localhost:5000/game").build();

        registerHandlers();
    }

    private void registerHandlers() {
        hub.on("Position", (Integer nx, Integer ny) -> {
            gameState.setPlayerPosition(nx, ny);
            waiting = false;
            notifyUI();
        }, Integer.class, Integer.class);

        hub.on("BombPlaced", (Integer bx, Integer by) -> {
            gameState.addBomb(bx, by, 1, "");
            notifyUI();
        }, Integer.class, Integer.class);

        hub.on("Exploded", (Integer bx, Integer by) -> {
            gameState.removeBomb(bx, by);
            gameState.addExplosion(bx, by);
            notifyUI();
        }, Integer.class, Integer.class);

        hub.on("ExplosionFinished", (Integer bx, Integer by) -> {
            gameState.removeExplosion(bx, by);
            notifyUI();
        }, Integer.class, Integer.class);

        hub.onClosed(error -> {
            ready = false;
            waiting = false;
        });
    }

    private void notifyUI() {
        SwingUtilities.invokeLater(onStateUpdated);
    }

    public void connect() {
        new Thread(() -> {
            try {
                hub.start().blockingAwait();
                ready = true;
                sendMove(gameState.getPlayerX(), gameState.getPlayerY());
            } catch (Exception e) {
                SwingUtilities.invokeLater(() ->
                        JOptionPane.showMessageDialog(null, "Nepavyko prisijungti prie serverio."));
            }
        }, "signalr-connect").start();
    }

    public void sendMove(int targetX, int targetY) {
        if (!ready || waiting) return;
        waiting = true;
        try {
            hub.send("Move", targetX, targetY);
        } catch (Exception e) {
            ready = false;
            waiting = false;
        }
    }

    public void sendPlaceBomb() {
        if (!ready) return;
        try {
            hub.send("PlaceBomb");
        } catch (Exception ignored) {}
    }

    public void disconnect() {
        ready = false;
        try { hub.stop().blockingAwait(); } catch (Exception ignored) {}
    }
}