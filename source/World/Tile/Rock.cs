using TestClient.Source.Physics;
using TestClient.Source.Render;

namespace TestClient.Source.World.Tile;

public class Rock : Block
{
    public Rock(int x) : base(x, 0)
    {
        
    }
    
    public override void RenderFace(Tessellator t, Level level, float x, float y, float z, int face)
    {
        var tex = GetTexture(0, level.GetMetadata(new BlockPos(x, y, z)));
        RenderFaceWithTex(t, level, x, y, z, face, tex);
    }
    
    protected override int GetTexture(int bottomMeta, int meta)
    {
        return meta switch
        {
            0 => TextureAtlas.Index("stone"),
            1 => TextureAtlas.Index("stone_granite"),
            2 => TextureAtlas.Index("stone_granite_smooth"),
            3 => TextureAtlas.Index("stone_diorite"),
            4 => TextureAtlas.Index("stone_diorite_smooth"),
            5 => TextureAtlas.Index("stone_andesite"),
            _ => TextureAtlas.Index("stone_andesite_smooth")
        };
    }
}