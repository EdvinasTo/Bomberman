package bomberman;

import bomberman.model.GameState;
import bomberman.network.NetworkManager;
import bomberman.ui.GamePanel;
import bomberman.sound.ClientSoundEvents;

import javax.swing.*;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;

public class Main {
    public static void main(String[] args) {
        SwingUtilities.invokeLater(() -> {
            GameState gameState = new GameState();
            ClientSoundEvents soundEvents = ClientSoundEvents.createDefault();

            // NetworkManager updates UI by calling panel.repaint() via callback
            //final GamePanel[] panelHolder = new GamePanel[1];
            NetworkManager networkManager = new NetworkManager(gameState, soundEvents);
            GamePanel panel = new GamePanel(gameState, networkManager);
            gameState.subscribe(panel);
            gameState.subscribe(soundEvents);

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
