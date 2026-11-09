package bomberman.ui.visual;

import bomberman.model.Bomb;
import java.awt.Color;
import java.awt.Graphics2D;

public final class StandardBombVisual extends BombVisual {
    @Override public void draw(Graphics2D g, Bomb bomb) {
        g.setColor(Color.BLACK);
        g.fillOval(bomb.getX() - 15, bomb.getY() - 15, 30, 30);
        g.setColor(new Color(120, 120, 120));
        g.fillOval(bomb.getX() - 8, bomb.getY() - 11, 8, 8);
    }
}
