namespace TestClient.Source.World.Tile;

using TestClient.Source.Render;

public class LogBlock : Block
{
    public LogBlock(int id) : base(id, TextureAtlas.Index("log_oak"))
    {
    }

    protected override int GetTexture(Level level, int face)
    {
        if (face == 0 || face == 1) return TextureAtlas.Index("log_oak_top");
        return TextureAtlas.Index("log_oak");
    }
}
