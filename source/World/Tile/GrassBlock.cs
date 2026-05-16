namespace TestClient.Source.World.Tile;

public class GrassBlock : Block
{
    public GrassBlock(int id) : base(id, 3)
    {
    }

    protected override int GetTexture(int face)
    {
        if (face == 1) return 0;
        return face == 0 ? 2 : 3;
    }
}