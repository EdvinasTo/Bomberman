package bomberman.ui.visual;

import java.awt.Graphics2D;

// Floor is traversable and has no server-side Block instance.
public abstract class BlockVisual {
    public abstract void drawIndestructible(Graphics2D g, int x, int y);
    public abstract void drawDestructible(Graphics2D g, int x, int y);
    public abstract void drawFloor(Graphics2D g, int x, int y);
}
