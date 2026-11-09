package bomberman.network;

import bomberman.model.GameState;
import bomberman.sound.ClientSoundEvents;
import bomberman.sound.GameEvent;

import javax.swing.JOptionPane;
import javax.swing.SwingUtilities;
import java.util.List;
import java.util.Objects;

/**
 * Bomberman networking without Maven or third-party libraries.
 * Uses Java 17's WebSocket and the SignalR JSON hub protocol implemented in
 * SignalRWebSocket. The C# GameHub API remains unchanged.
 */
public final class NetworkManager {
    private static final String DEFAULT_ENDPOINT = "ws://localhost:5000/game";

    private final GameState gameState;
    private final Runnable onStateUpdated;
    private final ClientSoundEvents soundEvents;
    private final String endpoint;
    private volatile SignalRWebSocket hub;
    private volatile boolean ready;
    private volatile String connectionStatus = "Connecting to server...";
    private volatile boolean gameFull;
    private volatile boolean closing;

    public NetworkManager(GameState gameState, Runnable onStateUpdated,
                          ClientSoundEvents soundEvents) {
        this(gameState, onStateUpdated, soundEvents,
                System.getProperty("bomberman.server", DEFAULT_ENDPOINT));
    }

    public NetworkManager(GameState gameState, Runnable onStateUpdated,
                          ClientSoundEvents soundEvents, String endpoint) {
        this.gameState = Objects.requireNonNull(gameState, "gameState");
        this.onStateUpdated = onStateUpdated;
        this.soundEvents = Objects.requireNonNull(soundEvents, "soundEvents");
        this.endpoint = Objects.requireNonNull(endpoint, "endpoint");
    }

    private static int integer(List<Object> args, int index) {
        Object result = args.get(index);
        if (!(result instanceof Number value)) {
            throw new IllegalArgumentException("Expected integer SignalR argument at index " + index);
        }
        return value.intValue();
    }

    private static String string(List<Object> args, int index) {
        Object result = args.get(index);
        if (!(result instanceof String text)) {
            throw new IllegalArgumentException("Expected text SignalR argument at index " + index);
        }
        return text;
    }

    private void onServerInvocation(String target, List<Object> args) {
        switch (target) {
            case "ArenaSnapshot" ->
                    gameState.applyArenaSnapshot(string(args, 0), integer(args, 1),
                            integer(args, 2), string(args, 3));
            case "PlayerSlot" -> gameState.setLocalPlayerSlot(integer(args, 0));
            case "GameFull" -> {
                gameFull = true;
                connectionStatus = "The game already has two players.";
                SwingUtilities.invokeLater(() -> JOptionPane.showMessageDialog(null,
                        "This game already has two players."));
            }
            case "Position" -> gameState.setPlayerPosition(integer(args, 0), integer(args, 1));
            case "PlayerHealth" -> gameState.setPlayerHealth(integer(args, 0));
            case "OtherPlayerSpawned" -> {
                String id = string(args, 0);
                gameState.updateOtherPlayer(id, integer(args, 1), integer(args, 2));
                gameState.setOtherPlayerSlot(id, integer(args, 3));
            }
            case "OtherPlayerPosition" -> gameState.updateOtherPlayer(
                    string(args, 0), integer(args, 1), integer(args, 2));
            case "PlayerLeft" -> gameState.removeOtherPlayer(string(args, 0));
            case "PlayerDied" -> {
                soundEvents.onGameEvent(GameEvent.PLAYER_DIED);
                SwingUtilities.invokeLater(() -> JOptionPane.showMessageDialog(null,
                        "You died! Restart the game to play again."));
            }
            case "BombPlaced" -> {
                gameState.addBomb(integer(args, 1), integer(args, 2), 60, string(args, 0));
                soundEvents.onGameEvent(GameEvent.BOMB_PLACED);
            }
            case "Exploded" -> {
                int x = integer(args, 1);
                int y = integer(args, 2);
                gameState.removeBomb(x, y);
                gameState.addExplosion(x, y, integer(args, 3));
                soundEvents.onGameEvent(GameEvent.EXPLOSION);
            }
            case "ExplosionFinished" ->
                    gameState.removeExplosion(integer(args, 1), integer(args, 2), 60);
            case "EnemyPosition" -> gameState.updateEnemyPosition(
                    string(args, 0), integer(args, 1), integer(args, 2));
            case "EnemySpawned" -> gameState.updateEnemy(
                    string(args, 0), integer(args, 1), integer(args, 2), integer(args, 3));
            case "EnemyHealth" -> gameState.updateEnemyHealth(string(args, 0), integer(args, 1));
            case "EnemyRemoved" -> gameState.removeEnemy(string(args, 0));
            case "BlockDestroyed" -> gameState.removeBlock(integer(args, 0), integer(args, 1));
            default -> System.out.println("Unknown SignalR event: " + target);
        }
        notifyUI();
    }

    private void notifyUI() {
        if (onStateUpdated != null) SwingUtilities.invokeLater(onStateUpdated);
    }

    public boolean isConnected() {
        return ready;
    }

    public String getConnectionStatus() {
        return connectionStatus;
    }

    public void connect() {
        Thread thread = new Thread(() -> {
            try {
                connectionStatus = "Connecting to " + endpoint;
                notifyUI();
                SignalRWebSocket connection = new SignalRWebSocket(
                        endpoint, this::onServerInvocation, this::onDisconnected);
                hub = connection;
                connection.connect();
                if (closing || gameFull) return;
                ready = true;
                connectionStatus = "Connected";
                notifyUI();
            } catch (Exception e) {
                if (closing) return;
                ready = false;
                connectionStatus = "Cannot connect to the server";
                System.err.println("Bomberman connection failed: " + e.getMessage());
                notifyUI();
                if (!gameFull) {
                    SwingUtilities.invokeLater(() -> JOptionPane.showMessageDialog(null,
                            "Cannot connect to server. Start the C# Server with 'dotnet run'.\n"
                                    + "Details: " + e.getMessage()));
                }
            }
        }, "signalr-connect");
        thread.setDaemon(true);
        thread.start();
    }

    private void onDisconnected(String reason) {
        ready = false;
        if (!closing && !gameFull) {
            connectionStatus = "Disconnected: " + reason;
            notifyUI();
            System.err.println("Bomberman server disconnected: " + reason);
        }
    }

    public void sendMove(int targetX, int targetY) {
        SignalRWebSocket connection = hub;
        if (ready && connection != null) connection.send("Move", targetX, targetY);
    }

    public void sendPlaceBomb() {
        SignalRWebSocket connection = hub;
        if (ready && connection != null) connection.send("PlaceBomb");
    }

    public void sendUndo() {
        SignalRWebSocket connection = hub;
        if (ready && connection != null) connection.send("Undo");
    }

    public void disconnect() {
        closing = true;
        ready = false;
        soundEvents.close();
        SignalRWebSocket connection = hub;
        if (connection != null) connection.close();
    }
}
