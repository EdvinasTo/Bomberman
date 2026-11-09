package bomberman.ui.visual;

public final class StandardVisualFactory implements IVisualFactory {
    @Override public PlayerVisual createPlayerVisual() { return new StandardPlayerVisual(); }
    @Override public BombVisual createBombVisual() { return new StandardBombVisual(); }
    @Override public BlockVisual createBlockVisual() { return new StandardBlockVisual(); }
    @Override public ExplosionVisual createExplosionVisual() { return new StandardExplosionVisual(); }
}
