using TestClient.Source.Physics;
using TestClient.Source.Render;

namespace TestClient.Source.World.Tile;

public class AirTile : Block
{
    public AirTile(int id) : base(id, 255)
    {
    }

    public override void Render(Tessellator t, Level level, BlockPos pos)
    {
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