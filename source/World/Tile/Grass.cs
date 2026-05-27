using TestClient.Source.Physics;
using TestClient.Source.Render;
using TestClient.Source.World.Biome;

namespace TestClient.Source.World.Tile;

public class Grass : Bush
{
    public Grass(int id) : base(id, TextureAtlas.Index("tallgrass"))
    {
    }

    protected override bool ShouldRenderFace(Level level, BlockPos pos)
    {
        return true;
    }

    protected override int GetBlockColor(Level level, BlockPos pos, int meta)
    {
        return BiomeColorHelper.GetGrassColorAtPos(level, pos);
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