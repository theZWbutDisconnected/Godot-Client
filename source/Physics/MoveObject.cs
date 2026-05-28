using TestClient.Source.World.Entities;

namespace TestClient.Source.Physics;

public enum MoveObjectType
{
    Miss,
    Block,
    Entity
}

public class MoveObject
{
    public MoveObjectType Type = MoveObjectType.Miss;

    public double HitX, HitY, HitZ;
    public Direction SideHit;
    public BlockPos BlockPos;
    public Entity EntityHit;

    public MoveObject() { }

    public MoveObject(MoveObjectType type, double hitX, double hitY, double hitZ,
        Direction side, BlockPos blockPos, Entity entity)
    {
        Type = type;
        HitX = hitX;
        HitY = hitY;
        HitZ = hitZ;
        SideHit = side;
        BlockPos = blockPos;
        EntityHit = entity;
    }

    public double DistanceSq(double ox, double oy, double oz)
    {
        double dx = HitX - ox, dy = HitY - oy, dz = HitZ - oz;
        return dx * dx + dy * dy + dz * dz;
    }

    public override string ToString()
    {
        return Type switch
        {
            MoveObjectType.Miss  => "MoveObject{MISS}",
            MoveObjectType.Block => $"MoveObject{{BLOCK at ({HitX:F6}, {HitY:F6}, {HitZ:F6}) side={SideHit} pos={BlockPos}}}",
            MoveObjectType.Entity => $"MoveObject{{ENTITY at ({HitX:F6}, {HitY:F6}, {HitZ:F6}) entity={EntityHit}}}",
            _ => "MoveObject{?}"
        };
    }
}
