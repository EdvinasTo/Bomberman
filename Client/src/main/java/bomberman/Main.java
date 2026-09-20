package bomberman;

import bomberman.model.GameState;
import bomberman.network.NetworkManager;
import bomberman.ui.GamePanel;

import javax.swing.*;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;

public class Main {
    public static void main(String[] args) {
        SwingUtilities.invokeLater(() -> {
            GameState gameState = new GameState();

            // NetworkManager updates UI by calling panel.repaint() via callback
            final GamePanel[] panelHolder = new GamePanel[1];
            NetworkManager networkManager = new NetworkManager(gameState, () -> {
                if (panelHolder[0] != null) {
                    panelHolder[0].repaint();
                }
            });

            GamePanel panel = new GamePanel(gameState, networkManager);
            panelHolder[0] = panel;

            JFrame frame = new JFrame("Bomberman Prototipas");
            frame.setDefaultCloseOperation(JFrame.DO_NOTHING_ON_CLOSE);
            frame.setContentPane(panel);
            frame.pack();
            frame.setResizable(false);
            frame.setLocationRelativeTo(null);

            frame.addWindowListener(new WindowAdapter() {
                @Override
                public void windowClosing(WindowEvent e) {
                    frame.dispose();
                    new Thread(() -> {
                        networkManager.disconnect();
                        System.exit(0);
                    }, "signalr-close").start();
                }
            });

            frame.setVisible(true);
            networkManager.connect();
        });
    }
}