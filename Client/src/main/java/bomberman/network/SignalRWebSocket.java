package bomberman.network;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.WebSocket;
import java.time.Duration;
import java.util.List;
import java.util.Map;
import java.util.Objects;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.CompletionStage;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;
import java.util.function.BiConsumer;
import java.util.function.Consumer;

/**
 * Small, dependency-free transport for ASP.NET Core SignalR's JSON hub protocol.
 * Requires only JDK 17 (java.net.http.WebSocket). Uses WebSocket-only transport
 * without the optional /negotiate request. This is not a general SignalR client:
 * it intentionally supports only the messages required by the Bomberman game.
 */
public final class SignalRWebSocket implements WebSocket.Listener, AutoCloseable {
    private static final char SEPARATOR = '\u001e';
    private final URI endpoint;
    private final BiConsumer<String, List<Object>> onInvocation;
    private final Consumer<String> onConnectionClosed;
    private final StringBuilder received = new StringBuilder();
    private final CompletableFuture<Void> handshakeComplete = new CompletableFuture<>();
    private final ScheduledExecutorService keepAlive = Executors.newSingleThreadScheduledExecutor(r -> {
        Thread thread = new Thread(r, "bomberman-signalr-keepalive");
        thread.setDaemon(true);
        return thread;
    });
    private final Object writeLock = new Object();
    private CompletableFuture<?> lastWrite = CompletableFuture.completedFuture(null);
    private volatile WebSocket socket;
    private volatile boolean connected;
    private volatile boolean closing;
    private volatile long lastSentNanos;

    public SignalRWebSocket(String url,
                            BiConsumer<String, List<Object>> onInvocation,
                            Consumer<String> onConnectionClosed) {
        this.endpoint = URI.create(url);
        this.onInvocation = Objects.requireNonNull(onInvocation, "onInvocation");
        this.onConnectionClosed = Objects.requireNonNull(onConnectionClosed, "onConnectionClosed");
        String scheme = endpoint.getScheme();
        if (!"ws".equalsIgnoreCase(scheme) && !"wss".equalsIgnoreCase(scheme)) {
            throw new IllegalArgumentException("SignalR endpoint must be ws:// or wss://");
        }
    }

    /** Blocking connect, called by NetworkManager on a background thread. */
    public void connect() throws Exception {
        if (closing) throw new IllegalStateException("Already closed");
        socket = HttpClient.newHttpClient().newWebSocketBuilder()
                .connectTimeout(Duration.ofSeconds(8))
                .buildAsync(endpoint, this)
                .get(10, TimeUnit.SECONDS);
        try {
            handshakeComplete.get(15, TimeUnit.SECONDS);
            if (closing) throw new IllegalStateException("Connection closed during handshake");
            connected = true;
            lastSentNanos = System.nanoTime();
            keepAlive.scheduleAtFixedRate(() -> {
                if (connected && System.nanoTime() - lastSentNanos >= TimeUnit.SECONDS.toNanos(12)) {
                    writeRecord("{\"type\":6}");
                }
            }, 5, 5, TimeUnit.SECONDS);
        } catch (Exception error) {
            close();
            throw error;
        }
    }

    public boolean isConnected() {
        return connected;
    }

    /** Fixed hub method names are written as JSON. Arguments are integer coordinates only. */
    public void send(String method, int... arguments) {
        if (!connected || closing) return;
        if (!"Move".equals(method) && !"PlaceBomb".equals(method) && !"Undo".equals(method)) {
            throw new IllegalArgumentException("Unsupported SignalR hub method: " + method);
        }
        StringBuilder json = new StringBuilder("{\"type\":1,\"target\":\"")
                .append(method).append("\",\"arguments\":[");
        for (int i = 0; i < arguments.length; i++) {
            if (i > 0) json.append(',');
            json.append(arguments[i]);
        }
        json.append("]}");
        writeRecord(json.toString());
    }

    private void writeRecord(String json) {
        WebSocket ws = socket;
        if (ws == null || closing) return;
        synchronized (writeLock) {
            lastWrite = lastWrite.handle((ignored, error) -> null)
                    .thenCompose(ignored -> ws.sendText(json + SEPARATOR, true));
            lastSentNanos = System.nanoTime();
        }
    }

    @Override
    public void onOpen(WebSocket webSocket) {
        this.socket = webSocket;
        webSocket.request(1);
        // SignalR JSON hub-protocol handshake must be the first application record.
        writeRecord("{\"protocol\":\"json\",\"version\":1}");
    }

    @Override
    public CompletionStage<?> onText(WebSocket webSocket, CharSequence data, boolean last) {
        try {
            synchronized (received) {
                received.append(data);
                if (received.length() > 2_000_000) {
                    throw new IllegalStateException("SignalR payload exceeds the size limit");
                }
                int separatorIndex;
                while ((separatorIndex = received.indexOf(String.valueOf(SEPARATOR))) >= 0) {
                    String record = received.substring(0, separatorIndex);
                    received.delete(0, separatorIndex + 1);
                    if (!record.isBlank()) processRecord(record);
                }
            }
        } catch (RuntimeException error) {
            System.err.println("Unable to parse SignalR server message: " + error.getMessage());
        } finally {
            webSocket.request(1);
        }
        return CompletableFuture.completedFuture(null);
    }

    private void processRecord(String record) {
        Map<String, Object> message = SignalRJson.parseMessage(record);
        if (!handshakeComplete.isDone()) {
            Object error = message.get("error");
            if (error != null) {
                handshakeComplete.completeExceptionally(new IllegalStateException(error.toString()));
            } else {
                handshakeComplete.complete(null);
            }
            return;
        }
        Object typeObj = message.get("type");
        if (!(typeObj instanceof Number number)) return;
        int type = number.intValue();
        if (type == 1) {
            Object target = message.get("target");
            Object args = message.get("arguments");
            if (target instanceof String name && args instanceof List<?> values) {
                @SuppressWarnings("unchecked")
                List<Object> typedArgs = (List<Object>) values;
                onInvocation.accept(name, typedArgs);
            }
        } else if (type == 7) {
            String error = String.valueOf(message.getOrDefault("error", "Server closed the connection"));
            onConnectionClosed.accept(error);
            close();
        } // type 6 Ping and type 3 Completion need no application action.
    }

    @Override
    public CompletionStage<?> onPing(WebSocket webSocket, java.nio.ByteBuffer message) {
        webSocket.request(1);
        return webSocket.sendPong(message);
    }

    @Override
    public CompletionStage<?> onPong(WebSocket webSocket, java.nio.ByteBuffer message) {
        webSocket.request(1);
        return CompletableFuture.completedFuture(null);
    }

    @Override
    public CompletionStage<?> onClose(WebSocket webSocket, int statusCode, String reason) {
        connected = false;
        if (!handshakeComplete.isDone()) {
            handshakeComplete.completeExceptionally(new IllegalStateException("Connection closed: " + reason));
        }
        if (!closing) onConnectionClosed.accept(reason.isBlank() ? "Server disconnected" : reason);
        close();
        return CompletableFuture.completedFuture(null);
    }

    @Override
    public void onError(WebSocket webSocket, Throwable error) {
        connected = false;
        handshakeComplete.completeExceptionally(error);
        if (!closing) onConnectionClosed.accept("Network error: " + error.getMessage());
        close();
    }

    @Override
    public void close() {
        closing = true;
        connected = false;
        keepAlive.shutdownNow();
        WebSocket ws = socket;
        if (ws != null) {
            try {
                ws.sendClose(WebSocket.NORMAL_CLOSURE, "bye");
            } catch (Exception ignored) {
                ws.abort();
            }
        }
    }
}
