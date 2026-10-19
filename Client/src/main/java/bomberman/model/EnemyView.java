package bomberman.model;

/** Client-side representation of an enemy received from the server. */
public final class EnemyView {
    private final String id;
    private volatile int x;
    private volatile int y;
    private volatile int health;

    public EnemyView(String id, int x, int y, int health) {
        this.id = id;
        this.x = x;
        this.y = y;
        this.health = health;
    }

    public String getId() { return id; }
    public int getX() { return x; }
    public int getY() { return y; }
    public int getHealth() { return health; }

    public void setPosition(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public void setHealth(int health) {
        this.health = Math.max(0, health);
    }
}
