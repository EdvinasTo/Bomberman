package bomberman.model;

public class Explosion {
    private final int x;
    private final int y;

    public Explosion(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public int getX() { return x; }
    public int getY() { return y; }

    public boolean isAt(int x, int y) {
        return this.x == x && this.y == y;
    }
}