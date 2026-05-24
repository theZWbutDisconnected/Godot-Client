namespace TestClient.Source.World.Tile;

using TestClient.Source.Render;

public class GrassBlock : Block
{
    public GrassBlock(int id) : base(id, TextureAtlas.Index("dirt"))
    {
    }

    protected override int GetTexture(int downMeta, int face)
    {
        if (face == 1) return TextureAtlas.Index("grass_top");
        return face == 0 ? TextureAtlas.Index("dirt") : TextureAtlas.Index("grass_side");
    }
}
