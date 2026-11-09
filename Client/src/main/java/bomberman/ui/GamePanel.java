
package bomberman.ui;

import bomberman.model.BlockView;
import bomberman.model.Bomb;
import bomberman.model.EnemyView;
import bomberman.model.Explosion;
import bomberman.model.GameState;
import bomberman.network.NetworkManager;
import bomberman.ui.visual.*;

import javax.swing.*;
import java.awt.*;
import java.awt.event.ActionEvent;
import java.util.Map;

public class GamePanel extends JPanel {
    private final GameState gameState;
    private final NetworkManager networkManager;
    private final Timer movementTimer;

    private IVisualFactory visualFactory;
    private PlayerVisual playerVisual;
    private BombVisual bombVisual;
    private BlockVisual blockVisual;
    private ExplosionVisual explosionVisual;
    private boolean neonTheme = false;

    private int moveDx;
    private int moveDy;

    private boolean left;
    private boolean right;
    private boolean up;
    private boolean down;

    public GamePanel(GameState gameState, NetworkManager networkManager) {
        this.gameState = gameState;
        this.networkManager = networkManager;

        setPreferredSize(new Dimension(660, 540));
        setBackground(Color.WHITE);
        setFocusable(true);

        movementTimer = new Timer(100, e -> sendMovement());
        setVisualFactory(new StandardVisualFactory());
        setupKeyBindings();
    }

    // Abstract Factory chooses a consistent family of graphics products.
    // Decorator adds 3 independent layers around the factory-created player.
    public final void setVisualFactory(IVisualFactory factory) {
        this.visualFactory = java.util.Objects.requireNonNull(factory, "factory");
        playerVisual = new PlayerCoordinatesDecorator(
                new PlayerNameDecorator(
                        new PlayerFrameDecorator(factory.createPlayerVisual())));
        bombVisual = factory.createBombVisual();
        blockVisual = factory.createBlockVisual();
        explosionVisual = factory.createExplosionVisual();
        neonTheme = factory instanceof NeonVisualFactory;
        repaint();
    }

    private void setupKeyBindings() {
        bindMoveKey("LEFT", "left", -5, 0);
        bindMoveKey("RIGHT", "right", 5, 0);
        bindMoveKey("UP", "up", 0, -5);
        bindMoveKey("DOWN", "down", 0, 5);

        getInputMap(WHEN_IN_FOCUSED_WINDOW)
                .put(KeyStroke.getKeyStroke("pressed SPACE"), "DROP_BOMB");
        getActionMap().put("DROP_BOMB", new AbstractAction() {
            @Override
            public void actionPerformed(ActionEvent e) {
                networkManager.sendPlaceBomb();
            }
        });

        // T switches the Abstract Factory graphics family at runtime.
        getInputMap(WHEN_IN_FOCUSED_WINDOW)
                .put(KeyStroke.getKeyStroke("pressed T"), "CHANGE_THEME");
        getActionMap().put("CHANGE_THEME", new AbstractAction() {
            @Override public void actionPerformed(ActionEvent e) {
                setVisualFactory(neonTheme
                        ? new StandardVisualFactory()
                        : new NeonVisualFactory());
            }
        });

        //UNDO
        getInputMap(WHEN_IN_FOCUSED_WINDOW)
                .put(KeyStroke.getKeyStroke("pressed Z"), "UNDO");
        getActionMap().put("UNDO", new AbstractAction() {
            @Override
            public void actionPerformed(ActionEvent e) {
                networkManager.sendUndo();
            }
        });
    }

    private void bindMoveKey(
            String key, String direction, int dx, int dy) {

        String pressed = direction + "_PRESSED";
        String released = direction + "_RELEASED";

        getInputMap(WHEN_IN_FOCUSED_WINDOW).put(
                KeyStroke.getKeyStroke("pressed " + key), pressed);

        getInputMap(WHEN_IN_FOCUSED_WINDOW).put(
                KeyStroke.getKeyStroke("released " + key), released);

        getActionMap().put(pressed, new AbstractAction() {
            @Override
            public void actionPerformed(ActionEvent e) {
                setDirection(direction, true);
                updateMovementDirection();
            }
        });

        getActionMap().put(released, new AbstractAction() {
            @Override
            public void actionPerformed(ActionEvent e) {
                setDirection(direction, false);
                updateMovementDirection();
            }
        });
    }

    private void setDirection(String direction, boolean pressed) {
        switch (direction) {
            case "left" -> left = pressed;
            case "right" -> right = pressed;
            case "up" -> up = pressed;
            case "down" -> down = pressed;
        }
    }

    private void updateMovementDirection() {
        moveDx = (right ? 5 : 0) - (left ? 5 : 0);
        moveDy = (down ? 5 : 0) - (up ? 5 : 0);

        if (moveDx == 0 && moveDy == 0) {
            movementTimer.stop();
        } else if (!movementTimer.isRunning()) {
            movementTimer.start();
        }
    }

    private void sendMovement() {
        if (moveDx == 0 && moveDy == 0) {
            movementTimer.stop();
            return;
        }

        networkManager.sendMove(
                gameState.getPlayerX() + moveDx,
                gameState.getPlayerY() + moveDy
        );
    }

    @Override
    protected void paintComponent(Graphics g) {
        super.paintComponent(g);

        Graphics2D g2 = (Graphics2D) g.create();

        try {
            // Draw all floor cells; the Builder uses null for walkable cells.
            for (int row = 0; row < gameState.getArenaHeight(); row++) {
                for (int col = 0; col < gameState.getArenaWidth(); col++) {
                    blockVisual.drawFloor(g2, col * 60, row * 60);
                }
            }

            // Render the arena blocks selected by the server-side Builder.
            for (BlockView block : gameState.getBlocks().values()) {
                if (block.isDestructible()) {
                    blockVisual.drawDestructible(g2, block.getX(), block.getY());
                } else {
                    blockVisual.drawIndestructible(g2, block.getX(), block.getY());
                }
            }

            synchronized (gameState.getBombs()) {
                for (Bomb bomb : gameState.getBombs()) {
                    bombVisual.draw(g2, bomb);
                }
            }

            synchronized (gameState.getExplosions()) {
                for (Explosion explosion : gameState.getExplosions()) {
                    explosionVisual.draw(g2, explosion);
                }
            }

            // Enemy rendering is unchanged; enemies are not factory products.
            for (EnemyView enemy : gameState.getEnemies().values()) {
                int x = enemy.getX();
                int y = enemy.getY();

                g2.setColor(new Color(180, 65, 210));
                g2.fillOval(x, y, 30, 30);

                g2.setColor(Color.DARK_GRAY);
                g2.setFont(new Font(Font.SANS_SERIF, Font.PLAIN, 11));
                g2.drawString("HP " + enemy.getHealth(), x, Math.max(55, y - 4));
            }

            // Both players use the same decorator chain, but distinct player slots.
            for (Map.Entry<String, Point> entry
                    : gameState.getOtherPlayers().entrySet()) {
                Point position = entry.getValue();
                int slot = gameState.getOtherPlayerSlot(entry.getKey());
                playerVisual.draw(g2, position.x, position.y, slot);
            }

            playerVisual.draw(g2,
                    gameState.getPlayerX(),
                    gameState.getPlayerY(),
                    gameState.getLocalPlayerSlot());

            // Health HUD.
            int health = Math.max(
                    0, Math.min(3, gameState.getPlayerHealth()));
            int maxHealth = 3;

            g2.setColor(new Color(0, 0, 0, 210));
            g2.fillRoundRect(8, 6, 165, 43, 12, 12);

            g2.setFont(new Font(
                    Font.SANS_SERIF, Font.BOLD, 15));
            g2.setColor(Color.WHITE);
            g2.drawString("HEALTH", 18, 23);

            int barX = 82;
            int barY = 14;
            int barWidth = 78;
            int barHeight = 12;

            g2.setColor(Color.GRAY);
            g2.fillRoundRect(
                    barX, barY, barWidth, barHeight, 6, 6);

            int filledWidth = barWidth * health / maxHealth;

            if (health == 3) {
                g2.setColor(new Color(50, 220, 100));
            } else if (health == 2) {
                g2.setColor(new Color(255, 190, 40));
            } else {
                g2.setColor(Color.RED);
            }

            if (filledWidth > 0) {
                g2.fillRoundRect(
                        barX, barY, filledWidth, barHeight, 6, 6);
            }

            g2.setColor(Color.WHITE);
            g2.setFont(new Font(
                    Font.SANS_SERIF, Font.BOLD, 12));
            g2.drawString(health + " / " + maxHealth, 82, 41);

            g2.setFont(new Font(Font.SANS_SERIF, Font.BOLD, 12));
            g2.setColor(neonTheme ? new Color(255, 230, 100) : Color.WHITE);
            g2.drawString("Arena: " + gameState.getArenaDifficulty()
                    + "   Theme: " + (neonTheme ? "Neon" : "Standard"), 210, 24);

            // A visible connection indicator makes startup failures easier to diagnose.
            if (!networkManager.isConnected()) {
                g2.setFont(new Font(Font.SANS_SERIF, Font.BOLD, 12));
                g2.setColor(new Color(40, 40, 40, 225));
                g2.fillRoundRect(112, getHeight() / 2 - 22,
                        getWidth() - 224, 44, 12, 12);
                g2.setColor(Color.WHITE);
                String status = networkManager.getConnectionStatus();
                FontMetrics metrics = g2.getFontMetrics();
                g2.drawString(status, (getWidth() - metrics.stringWidth(status)) / 2,
                        getHeight() / 2 + 5);
            }

        } finally {
            g2.dispose();
        }
    }
}
