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
        Level = level;
        _pos = pos;
        int x = pos.X, y = pos.Y, z = pos.Z;
        var tex = GetTexture(level.GetMetadata(pos));
        TextureAtlas.GetUV(tex, out var u0, out var v0, out var u1, out var v1);
        var rots = 2;
        t.Color(1.0F, 1.0F, 1.0F);

        for (var r = 0; r < rots; ++r)
        {
            var xa = (float)(Math.Sin(r * Math.PI / rots + Math.PI / 4D) * 0.5F);
            var za = (float)(Math.Cos(r * Math.PI / rots + Math.PI / 4D) * 0.5F);
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
    
    protected override int GetTexture(int meta)
    {
        var id = _pos.Offset(Direction.DOWN);
        GD.Print(meta);
        return meta switch
        {
            2 => TextureAtlas.Index("double_plant_grass_bottom"),
            10 => Level.GetMetadata(id) switch
            {
                2 => TextureAtlas.Index("double_plant_grass_top"),
                _ => 0
            },
            _ => 0
        };
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