using TestClient.Source.Render;

namespace TestClient.Source.World.Tile;

public class LogBlock : Block
{
    public LogBlock(int id) : base(id, TextureAtlas.Index("log_oak"))
    {
    }

    protected override int GetTexture(int downMeta, int face)
    {
        if (face == 0 || face == 1) return TextureAtlas.Index("log_oak_top");
        return TextureAtlas.Index("log_oak");
    }
}