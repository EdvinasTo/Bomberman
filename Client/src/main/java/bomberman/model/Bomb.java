package bomberman.model;

public class Bomb {
    private final int x;
    private final int y;
    private final int radius;
    private final String ownerId; // Useful when adding scores/kills

    public Bomb(int x, int y, int radius, String ownerId) {
        this.x = x;
        this.y = y;
        this.radius = radius;
        this.ownerId = ownerId;
    }

    public int getX() { return x; }
    public int getY() { return y; }
    public int getRadius() { return radius; }
    public String getOwnerId() { return ownerId; }

    // Helper for clearing matching bombs
    public boolean isAt(int x, int y) {
        return this.x == x && this.y == y;
    }
}