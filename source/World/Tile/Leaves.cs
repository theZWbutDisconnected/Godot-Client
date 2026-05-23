namespace TestClient.Source.World.Tile;

using TestClient.Source.Render;

public class Leaves : Block
{
    public Leaves(int id) : base(id, TextureAtlas.Index("leaves_oak"))
    {
    }

    public override bool IsOpaque()
    {
        return false;
    }
}
