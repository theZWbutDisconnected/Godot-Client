using TestClient.Source.Physics;
using TestClient.Source.Render;

namespace TestClient.Source.World.Tile;

public enum LiquidType
{
    FlowingWater,
    FlowingLava,
    Water,
    Lava
}

public class LiquidTile : Block
{
    LiquidType _type;
    
    public LiquidTile(int id, LiquidType type) : base(id, 255)
    {
        _type = type;
        TexId = type switch
        {
            LiquidType.Water => 14,
            LiquidType.Lava => 30,
            LiquidType.FlowingWater => 14,
            LiquidType.FlowingLava => 30,
            _ => 255
        };
    }
    
    public override void Render(Tessellator t, Level level, int layer, BlockPos pos)
    {
        Level = level;
        float c1;
        float c2;
        float c3;
        int x = pos.X, y = pos.Y, z = pos.Z;

        if (ShouldRenderFace(level, new BlockPos(x, y - 1, z), layer))
        {
            c1 = 1.0F;
            if (level.IsLit(x, y - 1, z) ^ (layer == 0))
                c1 *= 0.5F;
            t.Color(c1, c1, c1);
            t.Normal(0, -1, 0);
            RenderFace(t, x, y, z, 0);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y + 1, z), layer))
        {
            c1 = 1.0F;
            if (level.IsLit(x, y, z) ^ (layer == 0))
                c1 *= 0.5F;
            t.Color(c1, c1, c1);
            t.Normal(0, 1, 0);
            RenderFace(t, x, y, z, 1);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y, z - 1), layer))
        {
            c2 = 0.8F;
            if (level.IsLit(x, y, z - 1) ^ (layer == 0))
                c2 *= 0.5F;
            t.Color(c2, c2, c2);
            t.Normal(0, 0, -1);
            RenderFace(t, x, y, z, 2);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y, z + 1), layer))
        {
            c2 = 0.8F;
            if (level.IsLit(x, y, z + 1) ^ (layer == 0))
                c2 *= 0.5F;
            t.Color(c2, c2, c2);
            t.Normal(0, 0, 1);
            RenderFace(t, x, y, z, 3);
        }

        if (ShouldRenderFace(level, new BlockPos(x - 1, y, z), layer))
        {
            c3 = 0.6F;
            if (level.IsLit(x - 1, y, z) ^ (layer == 0))
                c3 *= 0.5F;
            t.Color(c3, c3, c3);
            t.Normal(-1, 0, 0);
            RenderFace(t, x, y, z, 4);
        }

        if (ShouldRenderFace(level, new BlockPos(x + 1, y, z), layer))
        {
            c3 = 0.6F;
            if (level.IsLit(x + 1, y, z) ^ (layer == 0))
                c3 *= 0.5F;
            t.Color(c3, c3, c3);
            t.Normal(1, 0, 0);
            RenderFace(t, x, y, z, 5);
        }
    }

    protected virtual bool ShouldRenderFace(Level level, BlockPos pos, int layer)
    {
        return !level.HasBlock(pos) && !(Blocks.GetPreset(level.GetBlockId(pos)) is LiquidTile);
    }
    
    public override void RenderFace(Tessellator t, int x, int y, int z, int face)
    {
        var tex = GetTexture(face);
        var u0 = tex % 16 / 16.0F;
        var u1 = u0 + 0.0624375F;
        var v0 = tex / 16 / 16.0F;
        var v1 = v0 + 0.0624375F;
        var x0 = x + 0.0F;
        var x1 = x + 1.0F;
        var y0 = y + 0.0F;
        var y1 = y + (!Level.HasBlock(new BlockPos(x, y + 1, z)) ? 0.87F : 1.0F);
        var z0 = z + 0.0F;
        var z1 = z + 1.0F;
        
        if (face == 0)
        {
            t.Normal(0, -1, 0);
            t.VertexUV(x0, y0, z0, u0, v0);
            t.VertexUV(x0, y0, z1, u0, v1);
            t.VertexUV(x1, y0, z1, u1, v1);
            t.VertexUV(x1, y0, z1, u1, v1);
            t.VertexUV(x1, y0, z0, u1, v0);
            t.VertexUV(x0, y0, z0, u0, v0);
        }

        if (face == 1)
        {
            t.Normal(0, 1, 0);
            t.VertexUV(x0, y1, z0, u0, v0);
            t.VertexUV(x1, y1, z0, u1, v0);
            t.VertexUV(x1, y1, z1, u1, v1);
            t.VertexUV(x1, y1, z1, u1, v1);
            t.VertexUV(x0, y1, z1, u0, v1);
            t.VertexUV(x0, y1, z0, u0, v0);
        }

        if (face == 2)
        {
            t.Normal(0, 0, -1);
            t.VertexUV(x0, y0, z0, u1, v1);
            t.VertexUV(x1, y0, z0, u0, v1);
            t.VertexUV(x1, y1, z0, u0, v0);
            t.VertexUV(x0, y0, z0, u1, v1);
            t.VertexUV(x1, y1, z0, u0, v0);
            t.VertexUV(x0, y1, z0, u1, v0);
        }

        if (face == 3)
        {
            t.Normal(0, 0, 1);
            t.VertexUV(x1, y1, z1, u1, v0);
            t.VertexUV(x1, y0, z1, u1, v1);
            t.VertexUV(x0, y0, z1, u0, v1);
            t.VertexUV(x1, y1, z1, u1, v0);
            t.VertexUV(x0, y0, z1, u0, v1);
            t.VertexUV(x0, y1, z1, u0, v0);
        }

        if (face == 4)
        {
            t.Normal(-1, 0, 0);
            t.VertexUV(x0, y0, z1, u0, v1);
            t.VertexUV(x0, y0, z0, u1, v1);
            t.VertexUV(x0, y1, z0, u1, v0);
            t.VertexUV(x0, y1, z0, u1, v0);
            t.VertexUV(x0, y1, z1, u0, v0);
            t.VertexUV(x0, y0, z1, u0, v1);
        }

        if (face == 5)
        {
            t.Normal(1, 0, 0);
            t.VertexUV(x1, y0, z0, u0, v1);
            t.VertexUV(x1, y0, z1, u1, v1);
            t.VertexUV(x1, y1, z1, u1, v0);
            t.VertexUV(x1, y1, z1, u1, v0);
            t.VertexUV(x1, y1, z0, u0, v0);
            t.VertexUV(x1, y0, z0, u0, v1);
        }
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