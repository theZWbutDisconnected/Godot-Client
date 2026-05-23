using TestClient.Source.Physics;
using TestClient.Source.Render;

namespace TestClient.Source.World.Tile;

public enum LiquidType
{
    Water,
    Lava
}

public class LiquidTile : Block
{
    public LiquidTile(int id, LiquidType type) : base(id, 255)
    {
        TexId = type switch
        {
            LiquidType.Water => 14,
            LiquidType.Lava => 30,
            _ => 255
        };
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