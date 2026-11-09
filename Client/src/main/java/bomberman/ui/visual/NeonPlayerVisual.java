package bomberman.ui.visual;

import java.awt.Color;
import java.awt.Graphics2D;

public final class NeonPlayerVisual extends PlayerVisual {
    @Override public void draw(Graphics2D g, int x, int y, int playerId) {
        g.setColor(playerId == 1 ? new Color(25, 230, 245) : new Color(255, 50, 175));
        g.fillRect(x, y, 30, 30);
        g.setColor(new Color(248, 248, 255));
        g.drawRect(x + 3, y + 3, 23, 23);
    }
}
