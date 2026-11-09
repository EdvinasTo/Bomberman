package bomberman.ui.visual;

import java.awt.Graphics2D;

// The shared abstract product; Decorator extends this same type.
public abstract class PlayerVisual {
    public abstract void draw(Graphics2D g, int x, int y, int playerId);
}
