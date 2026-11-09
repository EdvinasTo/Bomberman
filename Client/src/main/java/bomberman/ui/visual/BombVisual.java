package bomberman.ui.visual;

import bomberman.model.Bomb;
import java.awt.Graphics2D;

public abstract class BombVisual {
    public abstract void draw(Graphics2D g, Bomb bomb);
}
