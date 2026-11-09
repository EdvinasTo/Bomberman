package bomberman.ui.visual;

public interface IVisualFactory {
    PlayerVisual createPlayerVisual();
    BombVisual createBombVisual();
    BlockVisual createBlockVisual();
    ExplosionVisual createExplosionVisual();
}
