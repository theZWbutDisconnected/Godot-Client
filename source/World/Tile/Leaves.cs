using TestClient.Source.Physics;
using TestClient.Source.World.Biome;

namespace TestClient.Source.World.Tile;

using TestClient.Source.Render;

public class Leaves : Block
{
    public Leaves(int id) : base(id, TextureAtlas.Index("leaves_oak"))
    {
    }

    protected override uint GetBlockColor(Level level, BlockPos pos, int meta)
    {
        return (uint)BiomeColorHelper.GetFoliageColorAtPos(level, pos);
    }

    public override bool IsOpaque()
    {
        return false;
    }
}
