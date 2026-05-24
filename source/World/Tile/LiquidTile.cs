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

        float hCenter = GetFluidHeight(level, pos);
        float hSouth = GetFluidHeight(level, pos.South());
        float hSouthEast = GetFluidHeight(level, pos.South().East());
        float hEast = GetFluidHeight(level, pos.East());

        bool up = ShouldRenderFace(level, pos.Up());
        bool down = ShouldRenderFace(level, pos.Down());
        bool north = ShouldRenderFace(level, pos.North());
        bool south = ShouldRenderFace(level, pos.South());
        bool west = ShouldRenderFace(level, pos.West());
        bool east = ShouldRenderFace(level, pos.East());

        if (!up && !down && !north && !south && !west && !east)
            return;

        int tex = GetTexture(0, 0);
        TextureAtlas.GetUV(tex, out float u0, out float v0_tex, out float u1, out float v1_full);
        float uvTileHeight = v1_full - v0_tex;

        float x0 = x, x1 = x + 1.0F;
        float y0 = y, z0 = z, z1 = z + 1.0F;

        if (down)
        {
            float c1 = 1.0F;
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
            float y_cc = y0 + hCenter;   // (x,   z)   → center
            float y_cs = y0 + hSouth;    // (x,   z+1) → south
            float y_es = y0 + hSouthEast; // (x+1, z+1) → south-east
            float y_ce = y0 + hEast;     // (x+1, z)   → east

            float c1 = 1.0F;
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

        float c2 = 0.8F;
        float c3 = 0.6F;

        if (north)
        {
            float yn0 = y0 + hCenter;  // (x,   z)
            float yn1 = y0 + hEast;    // (x+1, z)
            float vt0 = v0_tex + uvTileHeight * hCenter;
            float vt1 = v0_tex + uvTileHeight * hEast;

            float cf = c2;
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
            float ys1 = y0 + hSouthEast; // (x+1, z+1)
            float ys0 = y0 + hSouth;     // (x,   z+1)
            float vt1 = v0_tex + uvTileHeight * hSouthEast;
            float vt0 = v0_tex + uvTileHeight * hSouth;

            float cf = c2;
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
            float yw1 = y0 + hSouth;  // (x, z+1)
            float yw0 = y0 + hCenter; // (x, z)
            float vt1 = v0_tex + uvTileHeight * hSouth;
            float vt0 = v0_tex + uvTileHeight * hCenter;

            float cf = c3;
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
            float ye0 = y0 + hEast;      // (x+1, z)
            float ye1 = y0 + hSouthEast; // (x+1, z+1)
            float vt0 = v0_tex + uvTileHeight * hEast;
            float vt1 = v0_tex + uvTileHeight * hSouthEast;

            float cf = c3;
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

    public override AABB GetCollision()
    {
        return null;
    }

    public override bool IsOpaque()
    {
        return false;
    }

    private float GetFluidHeight(Level level, BlockPos pos)
    {
        int count = 0;
        float sum = 0.0F;

        for (int j = 0; j < 4; ++j)
        {
            int dx = -(j & 1);
            int dz = -(j >> 1 & 1);
            // j=0: ( 0,  0), j=1: (-1,  0), j=2: ( 0, -1), j=3: (-1, -1)

            BlockPos neighbor = pos.Add(dx, 0, dz);

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
                    int meta = level.GetMetadata(neighbor);
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
