using System;
using TestClient.Source.Physics;
using TestClient.Source.Render;

namespace TestClient.Source.World.Tile;

public class Bush : Block
{
    public Bush(int id, int texId) : base(id, 15)
    {
        TexId = texId;
    }

    public override void Tick(Level level, int x, int y, int z, Random random)
    {
        var below = level.GetBlockId(new BlockPos(x, y - 1, z));
        if (!level.IsLit(x, y, z) || (below != Blocks.Dirt.Id && below != Blocks.Grass.Id))
            level.SetBlock(new BlockPos(x, y, z), 0);
    }

    public override void Render(Tessellator t, Level level, BlockPos pos)
    {
        int x = pos.X, y = pos.Y, z = pos.Z;
        var meta = level.GetMetadata(pos);
        var col = GetBlockColor(level, pos, meta);
        var r = ((col >> 16) & 0xFF) / 255.0f;
        var g = ((col >> 8) & 0xFF) / 255.0f;
        var b = (col & 0xFF) / 255.0f;
        var tex = GetTexture(0, meta);
        TextureAtlas.GetUV(tex, out var u0, out var v0, out var u1, out var v1);
        var rots = 2;
        t.Color(r, g, b);

        for (var d = 0; d < rots; ++d)
        {
            var xa = (float)(Math.Sin(d * Math.PI / rots + Math.PI / 4D) * 0.5F);
            var za = (float)(Math.Cos(d * Math.PI / rots + Math.PI / 4D) * 0.5F);
            var x0 = x + 0.5F - xa;
            var x1 = x + 0.5F + xa;
            var y0 = y + 0.0F;
            var y1 = y + 1.0F;
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

    public override AABB GetCollision(int meta)
    {
        return null;
    }

    public override AABB GetCube(int meta)
    {
        var f = 0.2F;
        return new AABB(0.5F - f, 0.0F, 0.5F - f, 0.5F + f, f * 3.0F, 0.5F + f);
    }

    public override bool IsOpaque()
    {
        return false;
    }
}