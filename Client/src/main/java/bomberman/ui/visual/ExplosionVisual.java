package bomberman.ui.visual;

import bomberman.model.Explosion;
import java.awt.Graphics2D;

public abstract class ExplosionVisual {
    public abstract void draw(Graphics2D g, Explosion explosion);
}
