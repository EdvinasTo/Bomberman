package bomberman.model;

public final class BlockView {
    private final int x;
    private final int y;
    private final boolean destructible;

    public BlockView(int x, int y, boolean destructible) {
        this.x = x;
        this.y = y;
        this.destructible = destructible;
    }

    public int getX() {
        return x;
    }

    public int getY() {
        return y;
    }

    public boolean isDestructible() {
        return destructible;
    }
}