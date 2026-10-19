
package bomberman.ui;

import bomberman.model.BlockView;
import bomberman.model.Bomb;
import bomberman.model.EnemyView;
import bomberman.model.Explosion;
import bomberman.model.GameState;
import bomberman.network.NetworkManager;

import javax.swing.*;
import java.awt.*;
import java.awt.event.ActionEvent;

public class GamePanel extends JPanel {
    private final GameState gameState;
    private final NetworkManager networkManager;
    private final Timer movementTimer;

    private int moveDx;
    private int moveDy;

    private boolean left;
    private boolean right;
    private boolean up;
    private boolean down;

    public GamePanel(GameState gameState, NetworkManager networkManager) {
        this.gameState = gameState;
        this.networkManager = networkManager;

        setPreferredSize(new Dimension(640, 480));
        setBackground(Color.WHITE);
        setFocusable(true);

        movementTimer = new Timer(100, e -> sendMovement());
        setupKeyBindings();
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
        // Blocks.
        for (BlockView block : gameState.getBlocks().values()) {
            g2.setColor(block.isDestructible()
                    ? new Color(160, 100, 45)
                    : new Color(90, 95, 105));

            g2.fillRect(block.getX(), block.getY(), 60, 60);

            if (block.isDestructible()) {
                g2.setColor(new Color(100, 55, 25));
                g2.drawLine(block.getX() + 8, block.getY() + 8,
                        block.getX() + 52, block.getY() + 52);
                g2.drawLine(block.getX() + 52, block.getY() + 8,
                        block.getX() + 8, block.getY() + 52);
            } else {
                g2.setColor(new Color(55, 60, 70));
                g2.drawRect(block.getX() + 3, block.getY() + 3, 54, 54);
            }
        }

        // Bombs.
        g2.setColor(Color.BLACK);
        synchronized (gameState.getBombs()) {
            for (Bomb bomb : gameState.getBombs()) {
                g2.fillOval(bomb.getX() - 15, bomb.getY() - 15, 30, 30);
            }
        }

        // Explosions.
        g2.setColor(Color.RED);
        synchronized (gameState.getExplosions()) {
            for (Explosion explosion : gameState.getExplosions()) {
                g2.fillRect(
                        explosion.getX() - 15,
                        explosion.getY() - 15,
                        30, 30
                );
            }
        }

        // Enemies and their health.
        for (EnemyView enemy : gameState.getEnemies().values()) {
            int x = enemy.getX();
            int y = enemy.getY();

            g2.setColor(new Color(180, 65, 210));
            g2.fillOval(x, y, 30, 30);
            
            g2.setColor(Color.DARK_GRAY);
            g2.setFont(new Font(Font.SANS_SERIF, Font.PLAIN, 11));
            g2.drawString("HP " + enemy.getHealth(), x, Math.max(55, y - 4));
        }

        // Player.
        g2.setColor(new Color(40, 121, 192));
        g2.fillRect(
                gameState.getPlayerX(),
                gameState.getPlayerY(),
                30, 30
        );

        // Health HUD: draw last so it stays visible.
        int health = Math.max(0, Math.min(3, gameState.getPlayerHealth()));
        int maxHealth = 3;

        g2.setColor(new Color(0, 0, 0, 210));
        g2.fillRoundRect(8, 6, 165, 43, 12, 12);

        g2.setFont(new Font(Font.SANS_SERIF, Font.BOLD, 15));
        g2.setColor(Color.WHITE);
        g2.drawString("HEALTH", 18, 23);

        int barX = 82;
        int barY = 14;
        int barWidth = 78;
        int barHeight = 12;

        g2.setColor(Color.GRAY);
        g2.fillRoundRect(barX, barY, barWidth, barHeight, 6, 6);

        int filledWidth = barWidth * health / maxHealth;

        if (health == 3) {
            g2.setColor(new Color(50, 220, 100));
        } else if (health == 2) {
            g2.setColor(new Color(255, 190, 40));
        } else {
            g2.setColor(Color.RED);
        }

        if (filledWidth > 0) {
            g2.fillRoundRect(barX, barY, filledWidth, barHeight, 6, 6);
        }

        g2.setColor(Color.WHITE);
        g2.setFont(new Font(Font.SANS_SERIF, Font.BOLD, 12));
        g2.drawString(health + " / " + maxHealth, 82, 41);

    } finally {
        g2.dispose();
    }
}

}
