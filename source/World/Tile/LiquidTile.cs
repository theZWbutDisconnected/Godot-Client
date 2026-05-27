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
            LiquidType.Water => TextureAtlas.Index("water_still"),
            LiquidType.Lava => TextureAtlas.Index("lava_still"),
            _ => 255
        };
    }

    public override void Render(Tessellator t, Level level, BlockPos pos)
    {
        int x = pos.X, y = pos.Y, z = pos.Z;

        var hCenter = GetFluidHeight(level, pos);
        var hSouth = GetFluidHeight(level, pos.South());
        var hSouthEast = GetFluidHeight(level, pos.South().East());
        var hEast = GetFluidHeight(level, pos.East());

        var up = ShouldRenderFace(level, pos.Up());
        var down = ShouldRenderFace(level, pos.Down());
        var north = ShouldRenderFace(level, pos.North());
        var south = ShouldRenderFace(level, pos.South());
        var west = ShouldRenderFace(level, pos.West());
        var east = ShouldRenderFace(level, pos.East());

        if (!up && !down && !north && !south && !west && !east)
            return;

        var tex = GetTexture(0, 0);
        TextureAtlas.GetUV(tex, out var u0, out var v0_tex, out var u1, out var v1_full);
        var uvTileHeight = v1_full - v0_tex;

        float x0 = x, x1 = x + 1.0F;
        float y0 = y, z0 = z, z1 = z + 1.0F;

        if (down)
        {
            var c1 = 1.0F;
            if (!level.IsLit(x, y - 1, z))
                c1 *= 0.5F;
            t.Color(c1, c1, c1);
            t.Normal(0, -1, 0);
            t.VertexUV(x0, y0, z0, u0, v0_tex);
            t.VertexUV(x0, y0, z1, u0, v1_full);
            t.VertexUV(x1, y0, z1, u1, v1_full);
            t.VertexUV(x1, y0, z1, u1, v1_full);
            t.VertexUV(x1, y0, z0, u1, v0_tex);
            t.VertexUV(x0, y0, z0, u0, v0_tex);
        }

        if (up)
        {
            var y_cc = y0 + hCenter; // (x,   z)   → center
            var y_cs = y0 + hSouth; // (x,   z+1) → south
            var y_es = y0 + hSouthEast; // (x+1, z+1) → south-east
            var y_ce = y0 + hEast; // (x+1, z)   → east

            var c1 = 1.0F;
            if (!level.IsLit(x, y, z))
                c1 *= 0.5F;
            t.Color(c1, c1, c1);
            t.Normal(0, 1, 0);
            t.VertexUV(x0, y_cc, z0, u0, v0_tex);
            t.VertexUV(x1, y_ce, z0, u1, v0_tex);
            t.VertexUV(x1, y_es, z1, u1, v1_full);
            t.VertexUV(x1, y_es, z1, u1, v1_full);
            t.VertexUV(x0, y_cs, z1, u0, v1_full);
            t.VertexUV(x0, y_cc, z0, u0, v0_tex);
        }

        var c2 = 0.8F;
        var c3 = 0.6F;

        if (north)
        {
            var yn0 = y0 + hCenter; // (x,   z)
            var yn1 = y0 + hEast; // (x+1, z)
            var vt0 = v0_tex + uvTileHeight * hCenter;
            var vt1 = v0_tex + uvTileHeight * hEast;

            var cf = c2;
            if (!level.IsLit(x, y, z - 1))
                cf *= 0.5F;
            t.Color(cf, cf, cf);
            t.Normal(0, 0, -1);
            t.VertexUV(x0, yn0, z0, u0, vt0);
            t.VertexUV(x1, yn1, z0, u1, vt1);
            t.VertexUV(x1, y0, z0, u1, v0_tex);
            t.VertexUV(x0, yn0, z0, u0, vt0);
            t.VertexUV(x1, y0, z0, u1, v0_tex);
            t.VertexUV(x0, y0, z0, u0, v0_tex);
        }

        if (south)
        {
            var ys1 = y0 + hSouthEast; // (x+1, z+1)
            var ys0 = y0 + hSouth; // (x,   z+1)
            var vt1 = v0_tex + uvTileHeight * hSouthEast;
            var vt0 = v0_tex + uvTileHeight * hSouth;

            var cf = c2;
            if (!level.IsLit(x, y, z + 1))
                cf *= 0.5F;
            t.Color(cf, cf, cf);
            t.Normal(0, 0, 1);
            t.VertexUV(x1, ys1, z1, u1, vt1);
            t.VertexUV(x1, y0, z1, u1, v0_tex);
            t.VertexUV(x0, y0, z1, u0, v0_tex);
            t.VertexUV(x1, ys1, z1, u1, vt1);
            t.VertexUV(x0, y0, z1, u0, v0_tex);
            t.VertexUV(x0, ys0, z1, u0, vt0);
        }

        if (west)
        {
            var yw1 = y0 + hSouth; // (x, z+1)
            var yw0 = y0 + hCenter; // (x, z)
            var vt1 = v0_tex + uvTileHeight * hSouth;
            var vt0 = v0_tex + uvTileHeight * hCenter;

            var cf = c3;
            if (!level.IsLit(x - 1, y, z))
                cf *= 0.5F;
            t.Color(cf, cf, cf);
            t.Normal(-1, 0, 0);
            t.VertexUV(x0, y0, z1, u0, v0_tex);
            t.VertexUV(x0, y0, z0, u1, v0_tex);
            t.VertexUV(x0, yw0, z0, u1, vt0);
            t.VertexUV(x0, yw0, z0, u1, vt0);
            t.VertexUV(x0, yw1, z1, u0, vt1);
            t.VertexUV(x0, y0, z1, u0, v0_tex);
        }

        if (east)
        {
            var ye0 = y0 + hEast; // (x+1, z)
            var ye1 = y0 + hSouthEast; // (x+1, z+1)
            var vt0 = v0_tex + uvTileHeight * hEast;
            var vt1 = v0_tex + uvTileHeight * hSouthEast;

            var cf = c3;
            if (!level.IsLit(x + 1, y, z))
                cf *= 0.5F;
            t.Color(cf, cf, cf);
            t.Normal(1, 0, 0);
            t.VertexUV(x1, y0, z0, u0, v0_tex);
            t.VertexUV(x1, y0, z1, u1, v0_tex);
            t.VertexUV(x1, ye1, z1, u1, vt1);
            t.VertexUV(x1, ye1, z1, u1, vt1);
            t.VertexUV(x1, ye0, z0, u0, vt0);
            t.VertexUV(x1, y0, z0, u0, v0_tex);
        }
    }

    protected override bool ShouldRenderFace(Level level, BlockPos pos)
    {
        var tile = Blocks.GetPreset(level.GetBlockId(pos));
        return (!level.HasBlock(pos) || !tile.IsOpaque()) && tile is not LiquidTile;
    }

    public override AABB GetCollision(int meta)
    {
        return null;
    }

    public override bool IsOpaque()
    {
        return false;
    }

    private float GetFluidHeight(Level level, BlockPos pos)
    {
        var count = 0;
        var sum = 0.0F;

        for (var j = 0; j < 4; ++j)
        {
            var dx = -(j & 1);
            var dz = -((j >> 1) & 1);
            // j=0: ( 0,  0), j=1: (-1,  0), j=2: ( 0, -1), j=3: (-1, -1)

            var neighbor = pos.Add(dx, 0, dz);

            if (level.HasBlock(neighbor.Up()))
            {
                var aboveTile = Blocks.GetPreset(level.GetBlockId(neighbor.Up()));
                if (aboveTile is LiquidTile)
                    return 1.0F;
            }

            if (!level.HasBlock(neighbor))
            {
                sum += 1.0F;
                count++;
            }
            else
            {
                var neighborTile = Blocks.GetPreset(level.GetBlockId(neighbor));
                if (neighborTile is LiquidTile)
                {
                    var meta = level.GetMetadata(neighbor);
                    if (meta >= 8 || meta == 0)
                    {
                        sum += GetLiquidHeightPercent(meta) * 10.0F;
                        count += 10;
                    }

                    sum += GetLiquidHeightPercent(meta);
                    count++;
                }
                else if (!neighborTile.IsOpaque())
                {
                    sum += 1.0F;
                    count++;
                }
            }
        }

        if (count == 0)
            return 1.0F;

        return 1.0F - sum / count;
    }

    private static float GetLiquidHeightPercent(int meta)
    {
        if (meta >= 8)
            meta = 0;
        return (meta + 1) / 9.0F;
    }
}