package bomberman.ui.visual;

import java.awt.Color;
import java.awt.Graphics2D;

public final class NeonBlockVisual extends BlockVisual {
    @Override public void drawFloor(Graphics2D g, int x, int y) {
        g.setColor(new Color(20, 25, 45));
        g.fillRect(x, y, 60, 60);
        g.setColor(new Color(42, 50, 75));
        g.drawRect(x, y, 59, 59);
    }

    @Override public void drawIndestructible(Graphics2D g, int x, int y) {
        g.setColor(new Color(40, 50, 82));
        g.fillRect(x, y, 60, 60);
        g.setColor(new Color(0, 225, 245));
        g.drawRect(x + 3, y + 3, 53, 53);
        g.drawLine(x + 3, y + 29, x + 56, y + 29);
    }

    @Override public void drawDestructible(Graphics2D g, int x, int y) {
        g.setColor(new Color(82, 34, 108));
        g.fillRect(x, y, 60, 60);
        g.setColor(new Color(255, 65, 194));
        g.drawRect(x + 3, y + 3, 53, 53);
        g.drawLine(x + 10, y + 10, x + 50, y + 50);
        g.drawLine(x + 50, y + 10, x + 10, y + 50);
    }
}
