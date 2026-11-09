package bomberman.ui.visual;

import bomberman.model.Bomb;
import java.awt.Color;
import java.awt.Graphics2D;

public final class NeonBombVisual extends BombVisual {
    @Override public void draw(Graphics2D g, Bomb bomb) {
        int x = bomb.getX() - 15;
        int y = bomb.getY() - 15;
        g.setColor(new Color(255, 210, 35));
        g.fillOval(x, y, 30, 30);
        g.setColor(new Color(255, 90, 5));
        g.drawOval(x + 3, y + 3, 23, 23);
        g.setColor(new Color(25, 24, 44));
        g.fillOval(x + 9, y + 9, 12, 12);
    }
}
