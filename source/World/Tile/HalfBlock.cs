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
        float y1 = y + 0.5F;

        if (ShouldRenderFace(level, new BlockPos(x, y - 1, z)))
        {
            c1 = 1.0F;
            if (!level.IsLit(x, y - 1, z))
                c1 *= 0.5F;
            t.Color(c1 * r, c1 * g, c1 * b);
            t.Normal(0, -1, 0);
            RenderHalfFace(t, x, y, z, 0, meta);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y + 1, z)))
        {
            c1 = 1.0F;
            if (!level.IsLit(x, y, z))
                c1 *= 0.5F;
            t.Color(c1 * r, c1 * g, c1 * b);
            t.Normal(0, 1, 0);
            RenderHalfFace(t, x, y, z, 1, meta);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y, z - 1)))
        {
            c2 = 0.8F;
            if (!level.IsLit(x, y, z - 1))
                c2 *= 0.5F;
            t.Color(c2 * r, c2 * g, c2 * b);
            t.Normal(0, 0, -1);
            RenderHalfFace(t, x, y, z, 2, meta);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y, z + 1)))
        {
            c2 = 0.8F;
            if (!level.IsLit(x, y, z + 1))
                c2 *= 0.5F;
            t.Color(c2 * r, c2 * g, c2 * b);
            t.Normal(0, 0, 1);
            RenderHalfFace(t, x, y, z, 3, meta);
        }

        if (ShouldRenderFace(level, new BlockPos(x - 1, y, z)))
        {
            c3 = 0.6F;
            if (!level.IsLit(x - 1, y, z))
                c3 *= 0.5F;
            t.Color(c3 * r, c3 * g, c3 * b);
            t.Normal(-1, 0, 0);
            RenderHalfFace(t, x, y, z, 4, meta);
        }

        if (ShouldRenderFace(level, new BlockPos(x + 1, y, z)))
        {
            c3 = 0.6F;
            if (!level.IsLit(x + 1, y, z))
                c3 *= 0.5F;
            t.Color(c3 * r, c3 * g, c3 * b);
            t.Normal(1, 0, 0);
            RenderHalfFace(t, x, y, z, 5, meta);
        }
    }

    private void RenderHalfFace(Tessellator t, float x, float y, float z, int face, int meta)
    {
        var tex = GetTexture(meta, face);
        TextureAtlas.GetUV(tex, out var u0, out var v0, out var u1, out var v1);
        var x0 = x + 0.0F;
        var x1 = x + 1.0F;
        var y0 = y + 0.0F;
        var y1 = y + 0.5F;
        var z0 = z + 0.0F;
        var z1 = z + 1.0F;

        var sideV0 = (v0 + v1) / 2;
        var sideV1 = v1;

        if (face == 0)
        {
            t.VertexUV(x0, y0, z0, u0, v0);
            t.VertexUV(x0, y0, z1, u0, v1);
            t.VertexUV(x1, y0, z1, u1, v1);
            t.VertexUV(x1, y0, z1, u1, v1);
            t.VertexUV(x1, y0, z0, u1, v0);
            t.VertexUV(x0, y0, z0, u0, v0);
        }

        if (face == 1)
        {
            t.VertexUV(x0, y1, z0, u0, v0);
            t.VertexUV(x1, y1, z0, u1, v0);
            t.VertexUV(x1, y1, z1, u1, v1);
            t.VertexUV(x1, y1, z1, u1, v1);
            t.VertexUV(x0, y1, z1, u0, v1);
            t.VertexUV(x0, y1, z0, u0, v0);
        }

        if (face == 2)
        {
            t.VertexUV(x0, y0, z0, u1, sideV1);
            t.VertexUV(x1, y0, z0, u0, sideV1);
            t.VertexUV(x1, y1, z0, u0, sideV0);
            t.VertexUV(x0, y0, z0, u1, sideV1);
            t.VertexUV(x1, y1, z0, u0, sideV0);
            t.VertexUV(x0, y1, z0, u1, sideV0);
        }

        if (face == 3)
        {
            t.VertexUV(x1, y1, z1, u1, sideV0);
            t.VertexUV(x1, y0, z1, u1, sideV1);
            t.VertexUV(x0, y0, z1, u0, sideV1);
            t.VertexUV(x1, y1, z1, u1, sideV0);
            t.VertexUV(x0, y0, z1, u0, sideV1);
            t.VertexUV(x0, y1, z1, u0, sideV0);
        }

        if (face == 4)
        {
            t.VertexUV(x0, y0, z1, u0, sideV1);
            t.VertexUV(x0, y0, z0, u1, sideV1);
            t.VertexUV(x0, y1, z0, u1, sideV0);
            t.VertexUV(x0, y1, z0, u1, sideV0);
            t.VertexUV(x0, y1, z1, u0, sideV0);
            t.VertexUV(x0, y0, z1, u0, sideV1);
        }

        if (face == 5)
        {
            t.VertexUV(x1, y0, z0, u0, sideV1);
            t.VertexUV(x1, y0, z1, u1, sideV1);
            t.VertexUV(x1, y1, z1, u1, sideV0);
            t.VertexUV(x1, y1, z1, u1, sideV0);
            t.VertexUV(x1, y1, z0, u0, sideV0);
            t.VertexUV(x1, y0, z0, u0, sideV1);
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
                _ => TextureAtlas.Index("nether_brick")
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

    public override AABB GetCollision()
    {
        return new AABB(0, 0, 0, 1, 0.5, 1);
    }
}