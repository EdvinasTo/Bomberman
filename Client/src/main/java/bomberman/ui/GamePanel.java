package bomberman.ui;

import bomberman.model.Bomb;
import bomberman.model.Explosion;
import bomberman.model.GameState;
import bomberman.network.NetworkManager;

import javax.swing.*;
import java.awt.*;
import java.awt.event.ActionEvent;

public class GamePanel extends JPanel {
    private final GameState gameState;
    private final NetworkManager networkManager;

    public GamePanel(GameState gameState, NetworkManager networkManager) {
        this.gameState = gameState;
        this.networkManager = networkManager;

        setPreferredSize(new Dimension(640, 480));
        setBackground(Color.WHITE);

        setupKeyBindings();
    }

    private void setupKeyBindings() {
        bindMoveKey("LEFT", -5, 0);
        bindMoveKey("RIGHT", 5, 0);
        bindMoveKey("UP", 0, -5);
        bindMoveKey("DOWN", 0, 5);

        // Bomb Key
        getInputMap(WHEN_IN_FOCUSED_WINDOW).put(KeyStroke.getKeyStroke("SPACE"), "DROP_BOMB");
        getActionMap().put("DROP_BOMB", new AbstractAction() {
            @Override
            public void actionPerformed(ActionEvent e) {
                networkManager.sendPlaceBomb();
            }
        });
    }

    private void bindMoveKey(String key, int dx, int dy) {
        getInputMap(WHEN_IN_FOCUSED_WINDOW).put(KeyStroke.getKeyStroke(key), key);
        getActionMap().put(key, new AbstractAction() {
            @Override
            public void actionPerformed(ActionEvent e) {
                networkManager.sendMove(gameState.getPlayerX() + dx, gameState.getPlayerY() + dy);
            }
        });
    }

    @Override
    protected void paintComponent(Graphics g) {
        super.paintComponent(g);

        // 1. Draw Bombs
        g.setColor(Color.BLACK);
        synchronized (gameState.getBombs()) {
            for (Bomb b : gameState.getBombs()) {
                g.fillOval(b.getX(), b.getY(), 30, 30);
            }
        }

        // 2. Draw Explosions
        g.setColor(Color.RED);
        synchronized (gameState.getExplosions()) {
            for (Explosion ex : gameState.getExplosions()) {
                g.fillRect(ex.getX(), ex.getY(), 30, 30);
            }
        }

        // 3. Draw Player
        g.setColor(new Color(40, 121, 192));
        g.fillRect(gameState.getPlayerX(), gameState.getPlayerY(), 30, 30);
    }
}