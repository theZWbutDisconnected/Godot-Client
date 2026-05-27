using TestClient.Source.Physics;
using TestClient.Source.Render;

namespace TestClient.Source.World.Tile;

public class FlowerBush : Bush
{
    public FlowerBush(int id) : base(id, 255)
    {
    }

    protected override bool ShouldRenderFace(Level level, BlockPos pos)
    {
        return true;
    }

    protected override int GetTexture(int downMeta, int meta)
    {
        return meta switch
        {
            0 => TextureAtlas.Index("flower_rose"),
            1 => TextureAtlas.Index("flower_blue_orchid"),
            2 => TextureAtlas.Index("flower_allium"),
            3 => TextureAtlas.Index("flower_houstonia"),
            4 => TextureAtlas.Index("flower_tulip_red"),
            5 => TextureAtlas.Index("flower_tulip_orange"),
            6 => TextureAtlas.Index("flower_tulip_white"),
            7 => TextureAtlas.Index("flower_tulip_pink"),
            _ => TextureAtlas.Index("flower_oxeye_daisy")
        };
    }

    public override AABB GetCollision(int meta)
    {
        return null;
    }

    public override bool IsOpaque()
    {
        return false;
    }
}