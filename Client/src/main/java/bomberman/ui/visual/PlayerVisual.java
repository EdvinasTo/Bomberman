package bomberman.ui.visual;

import java.awt.Graphics2D;


public abstract class PlayerVisual {
    public abstract void draw(Graphics2D g, int x, int y, int playerId);
}
