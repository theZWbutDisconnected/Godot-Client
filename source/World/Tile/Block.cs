using System;
using TestClient.Source.Physics;
using TestClient.Source.Render;

namespace TestClient.Source.World.Tile;

public class Block
{
    public int Id;
    public int TexId;

    protected Block(int id)
    {
        Id = id;
        Blocks.SetPreset(Id, this);
    }

    public Block(int id, int texId)
    {
        Id = id;
        TexId = texId;
        Blocks.SetPreset(Id, this);
    }

    public virtual void Render(Tessellator t, Level level, BlockPos pos)
    {
        float c1;
        float c2;
        float c3;
        int x = pos.X, y = pos.Y, z = pos.Z;

        if (ShouldRenderFace(level, new BlockPos(x, y - 1, z)))
        {
            c1 = 1.0F;
            if (!level.IsLit(x, y - 1, z))
                c1 *= 0.5F;
            t.Color(c1, c1, c1);
            t.Normal(0, -1, 0);
            RenderFace(t, level, x, y, z, 0);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y + 1, z)))
        {
            c1 = 1.0F;
            if (!level.IsLit(x, y, z))
                c1 *= 0.5F;
            t.Color(c1, c1, c1);
            t.Normal(0, 1, 0);
            RenderFace(t, level, x, y, z, 1);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y, z - 1)))
        {
            c2 = 0.8F;
            if (!level.IsLit(x, y, z - 1))
                c2 *= 0.5F;
            t.Color(c2, c2, c2);
            t.Normal(0, 0, -1);
            RenderFace(t, level, x, y, z, 2);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y, z + 1)))
        {
            c2 = 0.8F;
            if (!level.IsLit(x, y, z + 1))
                c2 *= 0.5F;
            t.Color(c2, c2, c2);
            t.Normal(0, 0, 1);
            RenderFace(t, level, x, y, z, 3);
        }

        if (ShouldRenderFace(level, new BlockPos(x - 1, y, z)))
        {
            c3 = 0.6F;
            if (!level.IsLit(x - 1, y, z))
                c3 *= 0.5F;
            t.Color(c3, c3, c3);
            t.Normal(-1, 0, 0);
            RenderFace(t, level, x, y, z, 4);
        }

        if (ShouldRenderFace(level, new BlockPos(x + 1, y, z)))
        {
            c3 = 0.6F;
            if (!level.IsLit(x + 1, y, z))
                c3 *= 0.5F;
            t.Color(c3, c3, c3);
            t.Normal(1, 0, 0);
            RenderFace(t, level, x, y, z, 5);
        }
    }

    protected virtual bool ShouldRenderFace(Level level, BlockPos pos)
    {
        return !level.HasBlock(pos) || !Blocks.GetPreset(level.GetBlockId(pos)).IsOpaque();
    }

    public virtual AABB GetCube()
    {
        return new AABB(0, 0, 0, 1, 1, 1);
    }

    public virtual AABB GetCollision()
    {
        return new AABB(0, 0, 0, 1, 1, 1);
    }

    public virtual bool IsOpaque()
    {
        return true;
    }

    protected virtual int GetTexture(Level level, int face)
    {
        return TexId;
    }

    public virtual void RenderFace(Tessellator t, Level level, int x, int y, int z, int face)
    {
        var tex = GetTexture(level, face);
        TextureAtlas.GetUV(tex, out var u0, out var v0, out var u1, out var v1);
        var x0 = x + 0.0F;
        var x1 = x + 1.0F;
        var y0 = y + 0.0F;
        var y1 = y + 1.0F;
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

    public virtual void Tick(Level level, int x, int y, int z, Random random) {
    }
}