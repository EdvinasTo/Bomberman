package bomberman.ui.visual;

public final class NeonVisualFactory implements IVisualFactory {
    @Override public PlayerVisual createPlayerVisual() { return new NeonPlayerVisual(); }
    @Override public BombVisual createBombVisual() { return new NeonBombVisual(); }
    @Override public BlockVisual createBlockVisual() { return new NeonBlockVisual(); }
    @Override public ExplosionVisual createExplosionVisual() { return new NeonExplosionVisual(); }
}
