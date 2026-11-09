package bomberman.ui.visual;

import java.awt.Color;
import java.awt.Font;
import java.awt.FontMetrics;
import java.awt.Graphics2D;

public final class PlayerNameDecorator extends PlayerVisualDecorator {
    public PlayerNameDecorator(PlayerVisual visual) { super(visual); }

    @Override public void draw(Graphics2D g, int x, int y, int playerId) {
        super.draw(g, x, y, playerId);
        String text = "P" + playerId;
        g.setFont(new Font(Font.SANS_SERIF, Font.BOLD, 13));
        FontMetrics fm = g.getFontMetrics();
        int tx = x + 15 - fm.stringWidth(text) / 2;
        int ty = y - 9;
        // Text shadow keeps the label readable against Standard and Neon themes.
        g.setColor(Color.BLACK);
        g.drawString(text, tx + 1, ty + 1);
        g.setColor(Color.WHITE);
        g.drawString(text, tx, ty);
    }
}
