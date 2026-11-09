package bomberman.ui.visual;

import java.awt.BasicStroke;
import java.awt.Color;
import java.awt.Graphics2D;
import java.awt.Stroke;

public final class PlayerFrameDecorator extends PlayerVisualDecorator {
    public PlayerFrameDecorator(PlayerVisual visual) { super(visual); }

    @Override public void draw(Graphics2D g, int x, int y, int playerId) {
        super.draw(g, x, y, playerId);
        Stroke previous = g.getStroke();
        g.setStroke(new BasicStroke(3f));
        g.setColor(new Color(255, 212, 50));
        g.drawRect(x - 3, y - 3, 35, 35);
        g.setStroke(previous);
    }
}
