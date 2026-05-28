using System;
using Godot;
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
        var meta = level.GetMetadata(pos);
        var col = GetBlockColor(level, pos, meta);
        var r = ((col >> 16) & 0xFF) / 255.0f;
        var g = ((col >> 8) & 0xFF) / 255.0f;
        var b = (col & 0xFF) / 255.0f;
        int x = pos.X, y = pos.Y, z = pos.Z;

        if (ShouldRenderFace(level, new BlockPos(x, y - 1, z)))
        {
            c1 = 1.0F;
            if (!level.IsLit(x, y - 1, z))
                c1 *= 0.5F;
            t.Color(c1 * r, c1 * g, c1 * b);
            t.Normal(0, -1, 0);
            RenderFace(t, level, x, y, z, 0);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y + 1, z)))
        {
            c1 = 1.0F;
            if (!level.IsLit(x, y, z))
                c1 *= 0.5F;
            t.Color(c1 * r, c1 * g, c1 * b);
            t.Normal(0, 1, 0);
            RenderFace(t, level, x, y, z, 1);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y, z - 1)))
        {
            c2 = 0.8F;
            if (!level.IsLit(x, y, z - 1))
                c2 *= 0.5F;
            t.Color(c2 * r, c2 * g, c2 * b);
            t.Normal(0, 0, -1);
            RenderFace(t, level, x, y, z, 2);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y, z + 1)))
        {
            c2 = 0.8F;
            if (!level.IsLit(x, y, z + 1))
                c2 *= 0.5F;
            t.Color(c2 * r, c2 * g, c2 * b);
            t.Normal(0, 0, 1);
            RenderFace(t, level, x, y, z, 3);
        }

        if (ShouldRenderFace(level, new BlockPos(x - 1, y, z)))
        {
            c3 = 0.6F;
            if (!level.IsLit(x - 1, y, z))
                c3 *= 0.5F;
            t.Color(c3 * r, c3 * g, c3 * b);
            t.Normal(-1, 0, 0);
            RenderFace(t, level, x, y, z, 4);
        }

        if (ShouldRenderFace(level, new BlockPos(x + 1, y, z)))
        {
            c3 = 0.6F;
            if (!level.IsLit(x + 1, y, z))
                c3 *= 0.5F;
            t.Color(c3 * r, c3 * g, c3 * b);
            t.Normal(1, 0, 0);
            RenderFace(t, level, x, y, z, 5);
        }
    }

    protected virtual int GetBlockColor(Level level, BlockPos pos, int meta)
    {
        return 0xFFFFFF;
    }

    protected virtual bool ShouldRenderFace(Level level, BlockPos pos)
    {
        Block block = Blocks.GetPreset(level.GetBlockId(pos));
        return !level.HasBlock(pos) || !block.IsOpaque() || !block.IsFullBlock();
    }

    public virtual AABB GetCube(int meta)
    {
        return GetCollision(meta);
    }

    public virtual AABB GetCollision(int meta)
    {
        return new AABB(0, 0, 0, 1, 1, 1);
    }

    public virtual bool IsFullBlock()
    {
        return true;
    }

    public virtual bool IsOpaque()
    {
        return true;
    }

    protected virtual int GetTexture(int bottomMeta, int face)
    {
        return TexId;
    }
    public virtual void RenderFace(Tessellator t, Level level, float x, float y, float z, int face)
    {
        var tex = GetTexture(0, face);
        RenderFaceWithTex(t, level, x, y, z, face, tex);
    }

    public virtual void RenderFaceWithTex(Tessellator t, Level level, float x, float y, float z, int face, int texId)
    {
        TextureAtlas.GetUV(texId, out var u0, out var v0, out var u1, out var v1);
        var x0 = x + 0.0F;
        var x1 = x + 1.0F;
        var y0 = y + 0.0F;
        var y1 = y + 1.0F;
        var z0 = z + 0.0F;
        var z1 = z + 1.0F;

        var orgCol = t.CurrentColor();
        int ix = (int)x, iy = (int)y, iz = (int)z;
        var lit = 0.5f;
        
        if (face == 0)
        {
            float ltLit = 1f, rtLit = 1f, lbLit = 1f, rbLit = 1f;
            if (IsOpaqueAt(level, ix - 1, iy - 1, iz - 1) || IsOpaqueAt(level, ix, iy - 1, iz - 1) || IsOpaqueAt(level, ix - 1, iy - 1, iz))
                ltLit = lit;
            if (IsOpaqueAt(level, ix + 1, iy - 1, iz - 1) || IsOpaqueAt(level, ix, iy - 1, iz - 1) || IsOpaqueAt(level, ix + 1, iy - 1, iz))
                rtLit = lit;
            if (IsOpaqueAt(level, ix - 1, iy - 1, iz + 1) || IsOpaqueAt(level, ix, iy - 1, iz + 1) || IsOpaqueAt(level, ix - 1, iy - 1, iz))
                lbLit = lit;
            if (IsOpaqueAt(level, ix + 1, iy - 1, iz + 1) || IsOpaqueAt(level, ix, iy - 1, iz + 1) || IsOpaqueAt(level, ix + 1, iy - 1, iz))
                rbLit = lit;

            t.Normal(0, -1, 0);
            SetVertex(t, orgCol, ltLit, x0, y0, z0, u0, v0);
            SetVertex(t, orgCol, lbLit, x0, y0, z1, u0, v1);
            SetVertex(t, orgCol, rbLit, x1, y0, z1, u1, v1);
            SetVertex(t, orgCol, rbLit, x1, y0, z1, u1, v1);
            SetVertex(t, orgCol, rtLit, x1, y0, z0, u1, v0);
            SetVertex(t, orgCol, ltLit, x0, y0, z0, u0, v0);
        }
        else if (face == 1)
        {
            float ltLit = 1f, rtLit = 1f, lbLit = 1f, rbLit = 1f;
            if (IsOpaqueAt(level, ix - 1, iy + 1, iz - 1) || IsOpaqueAt(level, ix, iy + 1, iz - 1) || IsOpaqueAt(level, ix - 1, iy + 1, iz))
                ltLit = lit;
            if (IsOpaqueAt(level, ix + 1, iy + 1, iz - 1) || IsOpaqueAt(level, ix, iy + 1, iz - 1) || IsOpaqueAt(level, ix + 1, iy + 1, iz))
                rtLit = lit;
            if (IsOpaqueAt(level, ix - 1, iy + 1, iz + 1) || IsOpaqueAt(level, ix, iy + 1, iz + 1) || IsOpaqueAt(level, ix - 1, iy + 1, iz))
                lbLit = lit;
            if (IsOpaqueAt(level, ix + 1, iy + 1, iz + 1) || IsOpaqueAt(level, ix, iy + 1, iz + 1) || IsOpaqueAt(level, ix + 1, iy + 1, iz))
                rbLit = lit;
            if (IsOpaqueAt(level, ix, iy + 1, iz))
                ltLit = lbLit = rtLit = rbLit = lit;

            t.Normal(0, 1, 0);
            SetVertex(t, orgCol, ltLit, x0, y1, z0, u0, v0);
            SetVertex(t, orgCol, rtLit, x1, y1, z0, u1, v0);
            SetVertex(t, orgCol, rbLit, x1, y1, z1, u1, v1);
            SetVertex(t, orgCol, rbLit, x1, y1, z1, u1, v1);
            SetVertex(t, orgCol, lbLit, x0, y1, z1, u0, v1);
            SetVertex(t, orgCol, ltLit, x0, y1, z0, u0, v0);
        }
        else if (face == 2)
        {
            float ltLit = 1f, rtLit = 1f, lbLit = 1f, rbLit = 1f;
            if (IsOpaqueAt(level, ix - 1, iy - 1, iz - 1) || IsOpaqueAt(level, ix, iy - 1, iz - 1) || IsOpaqueAt(level, ix - 1, iy, iz - 1))
                ltLit = lit;
            if (IsOpaqueAt(level, ix + 1, iy - 1, iz - 1) || IsOpaqueAt(level, ix, iy - 1, iz - 1) || IsOpaqueAt(level, ix + 1, iy, iz - 1))
                rtLit = lit;
            if (IsOpaqueAt(level, ix - 1, iy + 1, iz - 1) || IsOpaqueAt(level, ix, iy + 1, iz - 1) || IsOpaqueAt(level, ix - 1, iy, iz - 1))
                lbLit = lit;
            if (IsOpaqueAt(level, ix + 1, iy + 1, iz - 1) || IsOpaqueAt(level, ix, iy + 1, iz - 1) || IsOpaqueAt(level, ix + 1, iy, iz - 1))
                rbLit = lit;

            t.Normal(0, 0, -1);
            SetVertex(t, orgCol, ltLit, x0, y0, z0, u1, v1);
            SetVertex(t, orgCol, rtLit, x1, y0, z0, u0, v1);
            SetVertex(t, orgCol, rbLit, x1, y1, z0, u0, v0);
            SetVertex(t, orgCol, ltLit, x0, y0, z0, u1, v1);
            SetVertex(t, orgCol, rbLit, x1, y1, z0, u0, v0);
            SetVertex(t, orgCol, lbLit, x0, y1, z0, u1, v0);
        }
        else if (face == 3)
        {
            float ltLit = 1f, rtLit = 1f, lbLit = 1f, rbLit = 1f;
            if (IsOpaqueAt(level, ix + 1, iy + 1, iz + 1) || IsOpaqueAt(level, ix, iy + 1, iz + 1) || IsOpaqueAt(level, ix + 1, iy, iz + 1))
                rtLit = lit;
            if (IsOpaqueAt(level, ix + 1, iy - 1, iz + 1) || IsOpaqueAt(level, ix, iy - 1, iz + 1) || IsOpaqueAt(level, ix + 1, iy, iz + 1))
                rbLit = lit;
            if (IsOpaqueAt(level, ix - 1, iy - 1, iz + 1) || IsOpaqueAt(level, ix, iy - 1, iz + 1) || IsOpaqueAt(level, ix - 1, iy, iz + 1))
                lbLit = lit;
            if (IsOpaqueAt(level, ix - 1, iy + 1, iz + 1) || IsOpaqueAt(level, ix, iy + 1, iz + 1) || IsOpaqueAt(level, ix - 1, iy, iz + 1))
                ltLit = lit;

            t.Normal(0, 0, 1);
            SetVertex(t, orgCol, rtLit, x1, y1, z1, u1, v0);
            SetVertex(t, orgCol, rbLit, x1, y0, z1, u1, v1);
            SetVertex(t, orgCol, lbLit, x0, y0, z1, u0, v1);
            SetVertex(t, orgCol, rtLit, x1, y1, z1, u1, v0);
            SetVertex(t, orgCol, lbLit, x0, y0, z1, u0, v1);
            SetVertex(t, orgCol, ltLit, x0, y1, z1, u0, v0);
        }
        else if (face == 4)
        {
            float ltLit = 1f, rtLit = 1f, lbLit = 1f, rbLit = 1f;
            if (IsOpaqueAt(level, ix - 1, iy - 1, iz + 1) || IsOpaqueAt(level, ix - 1, iy - 1, iz) || IsOpaqueAt(level, ix - 1, iy, iz + 1))
                ltLit = lit;
            if (IsOpaqueAt(level, ix - 1, iy - 1, iz - 1) || IsOpaqueAt(level, ix - 1, iy - 1, iz) || IsOpaqueAt(level, ix - 1, iy, iz - 1))
                rtLit = lit;
            if (IsOpaqueAt(level, ix - 1, iy + 1, iz + 1) || IsOpaqueAt(level, ix - 1, iy + 1, iz) || IsOpaqueAt(level, ix - 1, iy, iz + 1))
                lbLit = lit;
            if (IsOpaqueAt(level, ix - 1, iy + 1, iz - 1) || IsOpaqueAt(level, ix - 1, iy + 1, iz) || IsOpaqueAt(level, ix - 1, iy, iz - 1))
                rbLit = lit;

            t.Normal(-1, 0, 0);
            SetVertex(t, orgCol, ltLit, x0, y0, z1, u0, v1);
            SetVertex(t, orgCol, rtLit, x0, y0, z0, u1, v1);
            SetVertex(t, orgCol, rbLit, x0, y1, z0, u1, v0);
            SetVertex(t, orgCol, rbLit, x0, y1, z0, u1, v0);
            SetVertex(t, orgCol, lbLit, x0, y1, z1, u0, v0);
            SetVertex(t, orgCol, ltLit, x0, y0, z1, u0, v1);
        }
        else if (face == 5)
        {
            float ltLit = 1f, rtLit = 1f, lbLit = 1f, rbLit = 1f;
            if (IsOpaqueAt(level, ix + 1, iy - 1, iz - 1) || IsOpaqueAt(level, ix + 1, iy - 1, iz) || IsOpaqueAt(level, ix + 1, iy, iz - 1))
                ltLit = lit;
            if (IsOpaqueAt(level, ix + 1, iy - 1, iz + 1) || IsOpaqueAt(level, ix + 1, iy - 1, iz) || IsOpaqueAt(level, ix + 1, iy, iz + 1))
                rtLit = lit;
            if (IsOpaqueAt(level, ix + 1, iy + 1, iz - 1) || IsOpaqueAt(level, ix + 1, iy + 1, iz) || IsOpaqueAt(level, ix + 1, iy, iz - 1))
                lbLit = lit;
            if (IsOpaqueAt(level, ix + 1, iy + 1, iz + 1) || IsOpaqueAt(level, ix + 1, iy + 1, iz) || IsOpaqueAt(level, ix + 1, iy, iz + 1))
                rbLit = lit;

            t.Normal(1, 0, 0);
            SetVertex(t, orgCol, ltLit, x1, y0, z0, u0, v1);
            SetVertex(t, orgCol, rtLit, x1, y0, z1, u1, v1);
            SetVertex(t, orgCol, rbLit, x1, y1, z1, u1, v0);
            SetVertex(t, orgCol, rbLit, x1, y1, z1, u1, v0);
            SetVertex(t, orgCol, lbLit, x1, y1, z0, u0, v0);
            SetVertex(t, orgCol, ltLit, x1, y0, z0, u0, v1);
        }
    }
    
    protected void SetVertex(Tessellator t, Color orgCol, float lit, float px, float py, float pz, float u, float v)
    {
        t.Color(orgCol.R, orgCol.G, orgCol.B);
        t.ColorMul(lit, lit, lit);
        t.VertexUV(px, py, pz, u, v);
    }

    protected static bool IsOpaqueAt(Level level, int bx, int by, int bz) => level.IsBlockOpaque(bx, by, bz);

    public virtual void Tick(Level level, int x, int y, int z, Random random)
    {
    }
}