﻿using Godot;
using TestClient.Source.Physics;
using TestClient.Source.Render;
using TestClient.Source.World.Biome;

namespace TestClient.Source.World.Tile;

public class HalfBlock : Block
{
    public HalfBlock(int id) : base(id, TextureAtlas.Index("stonebrick"))
    {
    }


    public override void Render(Tessellator t, Level level, BlockPos pos)
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

        float y1 = y;
        if (meta >= 8) y1 += 0.5f;

        if (ShouldRenderFace(level, new BlockPos(x, y - 1, z)))
        {
            c1 = 1.0F;
            if (!level.IsLit(x, y - 1, z))
                c1 *= 0.5F;
            t.Color(c1 * r, c1 * g, c1 * b);
            t.Normal(0, -1, 0);
            RenderHalfFace(t, level, x, y1, z, 0, meta);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y + 1, z)))
        {
            c1 = 1.0F;
            if (!level.IsLit(x, y, z))
                c1 *= 0.5F;
            t.Color(c1 * r, c1 * g, c1 * b);
            t.Normal(0, 1, 0);
            RenderHalfFace(t, level, x, y1, z, 1, meta);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y, z - 1)))
        {
            c2 = 0.8F;
            if (!level.IsLit(x, y, z - 1))
                c2 *= 0.5F;
            t.Color(c2 * r, c2 * g, c2 * b);
            t.Normal(0, 0, -1);
            RenderHalfFace(t, level, x, y1, z, 2, meta);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y, z + 1)))
        {
            c2 = 0.8F;
            if (!level.IsLit(x, y, z + 1))
                c2 *= 0.5F;
            t.Color(c2 * r, c2 * g, c2 * b);
            t.Normal(0, 0, 1);
            RenderHalfFace(t, level, x, y1, z, 3, meta);
        }

        if (ShouldRenderFace(level, new BlockPos(x - 1, y, z)))
        {
            c3 = 0.6F;
            if (!level.IsLit(x - 1, y, z))
                c3 *= 0.5F;
            t.Color(c3 * r, c3 * g, c3 * b);
            t.Normal(-1, 0, 0);
            RenderHalfFace(t, level, x, y1, z, 4, meta);
        }

        if (ShouldRenderFace(level, new BlockPos(x + 1, y, z)))
        {
            c3 = 0.6F;
            if (!level.IsLit(x + 1, y, z))
                c3 *= 0.5F;
            t.Color(c3 * r, c3 * g, c3 * b);
            t.Normal(1, 0, 0);
            RenderHalfFace(t, level, x, y1, z, 5, meta);
        }
    }

    private void RenderHalfFace(Tessellator t, Level level, float x, float y, float z, int face, int meta)
    {
        var tex = GetTexture(meta >= 8 ? meta - 8 : meta, face);
        TextureAtlas.GetUV(tex, out var u0, out var v0, out var u1, out var v1);
        var x0 = x + 0.0F;
        var x1 = x + 1.0F;
        var y0 = y + 0.0F;
        var y1 = y + 0.5F;
        var z0 = z + 0.0F;
        var z1 = z + 1.0F;

        var sideV0 = (v0 + v1) / 2;
        var sideV1 = v1;

        if (meta >= 8)
        {
            var org0 = sideV0;
            var org1 = sideV1;
            sideV0 -= org1 - org0;
            sideV1 -= org1 - org0;
        }

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

        if (face == 1)
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

        if (face == 2)
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
            SetVertex(t, orgCol, ltLit, x0, y0, z0, u1, sideV1);
            SetVertex(t, orgCol, rtLit, x1, y0, z0, u0, sideV1);
            SetVertex(t, orgCol, rbLit, x1, y1, z0, u0, sideV0);
            SetVertex(t, orgCol, ltLit, x0, y0, z0, u1, sideV1);
            SetVertex(t, orgCol, rbLit, x1, y1, z0, u0, sideV0);
            SetVertex(t, orgCol, lbLit, x0, y1, z0, u1, sideV0);
        }

        if (face == 3)
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
            SetVertex(t, orgCol, rtLit, x1, y1, z1, u1, sideV0);
            SetVertex(t, orgCol, rbLit, x1, y0, z1, u1, sideV1);
            SetVertex(t, orgCol, lbLit, x0, y0, z1, u0, sideV1);
            SetVertex(t, orgCol, rtLit, x1, y1, z1, u1, sideV0);
            SetVertex(t, orgCol, lbLit, x0, y0, z1, u0, sideV1);
            SetVertex(t, orgCol, ltLit, x0, y1, z1, u0, sideV0);
        }

        if (face == 4)
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
            SetVertex(t, orgCol, ltLit, x0, y0, z1, u0, sideV1);
            SetVertex(t, orgCol, rtLit, x0, y0, z0, u1, sideV1);
            SetVertex(t, orgCol, rbLit, x0, y1, z0, u1, sideV0);
            SetVertex(t, orgCol, rbLit, x0, y1, z0, u1, sideV0);
            SetVertex(t, orgCol, lbLit, x0, y1, z1, u0, sideV0);
            SetVertex(t, orgCol, ltLit, x0, y0, z1, u0, sideV1);
        }

        if (face == 5)
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
            SetVertex(t, orgCol, ltLit, x1, y0, z0, u0, sideV1);
            SetVertex(t, orgCol, rtLit, x1, y0, z1, u1, sideV1);
            SetVertex(t, orgCol, rbLit, x1, y1, z1, u1, sideV0);
            SetVertex(t, orgCol, rbLit, x1, y1, z1, u1, sideV0);
            SetVertex(t, orgCol, lbLit, x1, y1, z0, u0, sideV0);
            SetVertex(t, orgCol, ltLit, x1, y0, z0, u0, sideV1);
        }
    }
    
    protected override int GetTexture(int meta, int face)
    {
        if (Id == 44) {
            return meta switch
            {
                0 => face switch
                {
                    0 | 1 => TextureAtlas.Index("stone_slab_top"),
                    _ => TextureAtlas.Index("stone_slab_side"),
                },
                1 => face switch
                {
                    0 => TextureAtlas.Index("sandstone_bottom"),
                    1 => TextureAtlas.Index("sandstone_top"),
                    _ => TextureAtlas.Index("sandstone_normal"),
                },
                3 => TextureAtlas.Index("cobblestone"),
                4 => TextureAtlas.Index("brick"),
                5 => TextureAtlas.Index("stonebrick"),
                6 => TextureAtlas.Index("nether_brick"),
                _ => face switch
                {
                    0 | 1 => TextureAtlas.Index("quartz_block_top"),
                    _ => TextureAtlas.Index("quartz_block_side"),
                }
            };
        }

        if (Id == 126)
        {
            return meta switch
            {
                0 => TextureAtlas.Index("planks_oak"),
                1 => TextureAtlas.Index("planks_spruce"),
                2 => TextureAtlas.Index("planks_birch"),
                3 => TextureAtlas.Index("planks_jungle"),
                4 => TextureAtlas.Index("planks_acacia"),
                _ => TextureAtlas.Index("planks_big_oak")
            };
        }

        return 0;
    }

    public override bool IsFullBlock()
    {
        return false;
    }

    public override AABB GetCollision(int meta)
    {
        return new AABB(0, meta >= 8 ? 0.5 : 0, 0, 1, meta >= 8 ? 1 : 0.5, 1);
    }
}