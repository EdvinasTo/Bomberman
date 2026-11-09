package bomberman.ui.visual;

import java.awt.Graphics2D;
import java.util.Objects;

// Both is-a PlayerVisual and has-a PlayerVisual (wrapped association).
public abstract class PlayerVisualDecorator extends PlayerVisual {
    protected final PlayerVisual wrapped;

    protected PlayerVisualDecorator(PlayerVisual visual) {
        this.wrapped = Objects.requireNonNull(visual, "wrapped");
    }

    @Override public void draw(Graphics2D g, int x, int y, int playerId) {
        wrapped.draw(g, x, y, playerId);
    }
}
