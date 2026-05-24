using TestClient.Source.Render;

namespace TestClient.Source.World.Tile;

public class AirTile : Block
{
    public AirTile(int id) : base(id, TextureAtlas.Index("missing_tex"))
    {
    }

    public override bool IsOpaque()
    {
        return true;
    }
}