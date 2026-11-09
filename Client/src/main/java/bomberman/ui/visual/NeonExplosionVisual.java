package bomberman.ui.visual;

import bomberman.model.Explosion;
import java.awt.Color;
import java.awt.Graphics2D;

public final class NeonExplosionVisual extends ExplosionVisual {
    @Override public void draw(Graphics2D g, Explosion explosion) {
        int x = explosion.getX() - 15;
        int y = explosion.getY() - 15;
        g.setColor(new Color(255, 35, 170, 225));
        g.fillRect(x, y, 30, 30);
        g.setColor(new Color(255, 248, 110));
        g.fillOval(x + 7, y + 7, 16, 16);
    }
}
