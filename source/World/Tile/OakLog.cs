namespace TestClient.Source.World.Tile;

public class OakLog : Block
{
    public OakLog(int id) : base(id, 3)
    {
        
    }

    protected override int GetTexture(int face) {
        if (face == 0 || face == 1) {
            return 21;
        }
        return 20;
    }
}