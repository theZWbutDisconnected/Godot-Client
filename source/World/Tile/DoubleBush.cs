using System;
using Godot;
using TestClient.Source.Physics;
using TestClient.Source.Render;

namespace TestClient.Source.World.Tile;

public class DoubleBush : Block
{
    private BlockPos _pos;
    public DoubleBush(int id, int texId) : base(id, 15)
    {
        TexId = texId;
    }

    public override void Tick(Level level, int x, int y, int z, Random random)
    {
    }

    public override void Render(Tessellator t, Level level, BlockPos pos)
    {
        _pos = pos;
        float x = pos.X, y = pos.Y, z = pos.Z;
        var meta = level.GetMetadata(pos);
        var metadown = level.GetMetadata(_pos.Offset(Direction.DOWN));
        var tex = GetTexture(metadown, meta);
        TextureAtlas.GetUV(tex, out var u0, out var v0, out var u1, out var v1);
        var rots = 2;
        t.Color(1.0F, 1.0F, 1.0F);

        if (meta == 10 && metadown == 0)
        {
            var frontTex = TextureAtlas.Index("double_plant_sunflower_front");
            var backTex = TextureAtlas.Index("double_plant_sunflower_back");
            TextureAtlas.GetUV(backTex, out var fu0, out var fv0, out var fu1, out var fv1);
            TextureAtlas.GetUV(frontTex, out var bu0, out var bv0, out var bu1, out var bv1);

            float angle = 22.5f * Mathf.Pi / 180f;
            float cosA = Mathf.Cos(angle);
            float sinA = Mathf.Sin(angle);

            float cx = 9.6f / 16f - 0.5f;
            float hY0 = -1f / 16f - 0.5f;
            float hY1 = 15f / 16f - 0.5f;
            float hZ0 = 1f / 16f;
            float hZ1 = 15f / 16f;

            float rx0 = cx * cosA - hY0 * sinA;
            float ry0 = cx * sinA + hY0 * cosA;
            float rx1 = cx * cosA - hY1 * sinA;
            float ry1 = cx * sinA + hY1 * cosA;

            float hx0 = x + 0.5f + rx0;
            float hy0 = y + 0.5f + ry0;
            float hx1 = x + 0.5f + rx1;
            float hy1 = y + 0.5f + ry1;

            t.Color(1.0F, 1.0F, 1.0F);

            t.VertexUV(hx1, hy1, hZ0 + z, fu0, fv0);
            t.VertexUV(hx1, hy1, hZ1 + z, fu1, fv0);
            t.VertexUV(hx0, hy0, hZ1 + z, fu1, fv1);
            t.VertexUV(hx1, hy1, hZ0 + z, fu0, fv0);
            t.VertexUV(hx0, hy0, hZ1 + z, fu1, fv1);
            t.VertexUV(hx0, hy0, hZ0 + z, fu0, fv1);

            t.VertexUV(hx1, hy1, hZ1 + z, bu1, fv0);
            t.VertexUV(hx1, hy1, hZ0 + z, bu0, fv0);
            t.VertexUV(hx0, hy0, hZ0 + z, bu0, fv1);
            t.VertexUV(hx1, hy1, hZ1 + z, bu1, fv0);
            t.VertexUV(hx0, hy0, hZ0 + z, bu0, fv1);
            t.VertexUV(hx0, hy0, hZ1 + z, bu1, fv1);
        }

        if (meta == 10 && metadown == 0) y -= 0.125f;
        for (var r = 0; r < rots; ++r)
        {
            var xa = (float)(Math.Sin(r * Math.PI / rots + Math.PI / 4D) * 0.5F);
            var za = (float)(Math.Cos(r * Math.PI / rots + Math.PI / 4D) * 0.5F);
            var x0 = x + 0.5F - xa;
            var x1 = x + 0.5F + xa;
            var y0 = y + 0.0F;
            var y1 = y + 1.0F;
            if (meta == 10 && metadown == 0 || meta == 0) y1 -= 0.125f;
            var z0 = z + 0.5F - za;
            var z1 = z + 0.5F + za;
            t.VertexUV(x0, y1, z0, u1, v0);
            t.VertexUV(x1, y1, z1, u0, v0);
            t.VertexUV(x1, y0, z1, u0, v1);
            t.VertexUV(x0, y1, z0, u1, v0);
            t.VertexUV(x1, y0, z1, u0, v1);
            t.VertexUV(x0, y0, z0, u1, v1);
            t.VertexUV(x1, y1, z1, u0, v0);
            t.VertexUV(x0, y1, z0, u1, v0);
            t.VertexUV(x0, y0, z0, u1, v1);
            t.VertexUV(x1, y1, z1, u0, v0);
            t.VertexUV(x0, y0, z0, u1, v1);
            t.VertexUV(x1, y0, z1, u0, v1);
        }
    }

    protected override bool ShouldRenderFace(Level level, BlockPos pos)
    {
        return true;
    }
    
    protected override int GetTexture(int downMeta, int meta)
    {
        var name = GetPlantTexture(meta == 10 ? downMeta : meta);
        return meta switch
        {
            10 => downMeta switch
            {
                _ => TextureAtlas.Index(name + "_top")
            },
            _=> TextureAtlas.Index(name + "_bottom")
        };
    }

    protected virtual string GetPlantTexture(int meta)
    {
        var plantName = meta switch
        {
            0 => "double_plant_sunflower",
            1 => "double_plant_syringa",
            2 => "double_plant_grass",
            3 => "double_plant_fern",
            4 => "double_plant_rose",
            _ => "double_plant_paeonia"
        };
        return plantName;
    }

    public override AABB GetCollision()
    {
        return null;
    }

    public override AABB GetCube()
    {
        float f = 0.4F;
        return new AABB(0.5F - f, 0.0F, 0.5F - f, 0.5F + f, 0.8F, 0.5F + f);
    }

    public override bool IsOpaque()
    {
        return false;
    }
}