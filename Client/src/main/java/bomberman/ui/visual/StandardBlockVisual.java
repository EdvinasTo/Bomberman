package bomberman.ui.visual;

import java.awt.Color;
import java.awt.Graphics2D;

public final class StandardBlockVisual extends BlockVisual {
    @Override public void drawFloor(Graphics2D g, int x, int y) {
        g.setColor(new Color(246, 246, 246));
        g.fillRect(x, y, 60, 60);
        g.setColor(new Color(230, 230, 230));
        g.drawRect(x, y, 59, 59);
    }

    @Override public void drawIndestructible(Graphics2D g, int x, int y) {
        g.setColor(new Color(90, 95, 105));
        g.fillRect(x, y, 60, 60);
        g.setColor(new Color(55, 60, 70));
        g.drawRect(x + 3, y + 3, 53, 53);
    }

    @Override public void drawDestructible(Graphics2D g, int x, int y) {
        g.setColor(new Color(160, 100, 45));
        g.fillRect(x, y, 60, 60);
        g.setColor(new Color(100, 55, 25));
        g.drawLine(x + 8, y + 8, x + 52, y + 52);
        g.drawLine(x + 52, y + 8, x + 8, y + 52);
    }
}
