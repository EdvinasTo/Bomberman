package bomberman.ui.visual;

import bomberman.model.Explosion;
import java.awt.Color;
import java.awt.Graphics2D;

public final class StandardExplosionVisual extends ExplosionVisual {
    @Override public void draw(Graphics2D g, Explosion explosion) {
        g.setColor(new Color(230, 65, 28, 210));
        g.fillRect(explosion.getX() - 15, explosion.getY() - 15, 30, 30);
        g.setColor(new Color(255, 210, 65));
        g.fillRect(explosion.getX() - 7, explosion.getY() - 7, 14, 14);
    }
}
