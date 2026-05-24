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

    protected override uint GetBlockColor(Level level, BlockPos pos, int meta)
    {
        return (uint)BiomeColorHelper.GetGrassColorAtPos(level, pos);
    }

    public override AABB GetCollision()
    {
        return null;
    }

    public override bool IsOpaque()
    {
        return false;
    }
}