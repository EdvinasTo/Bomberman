namespace Bomberman.Server.src.Builder;

public interface IArenaBuilder
{
    void Reset();
    void SetSize(int width, int height);
    void AddOuterWalls();
    void AddInnerWalls();
    void AddDestructibleBlocks();
    void SetPlayerSpawns();
    Arena GetResult();
}
