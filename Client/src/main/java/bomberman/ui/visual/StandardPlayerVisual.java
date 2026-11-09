package bomberman.ui.visual;

import java.awt.Color;
import java.awt.Graphics2D;

public final class StandardPlayerVisual extends PlayerVisual {
    @Override public void draw(Graphics2D g, int x, int y, int playerId) {
        g.setColor(playerId == 1 ? new Color(40, 121, 192) : new Color(235, 135, 45));
        g.fillRect(x, y, 30, 30);
    }
}
