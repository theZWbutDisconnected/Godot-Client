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
        int below = level.GetBlockId(new BlockPos(x, y - 1, z));
        if (!level.IsLit(x, y, z) || below != Blocks.Dirt.Id && below != Blocks.Grass.Id) {
            level.SetBlock(new BlockPos(x, y, z), 0);
        }
    }

    public override void Render(Tessellator t, Level level, int layer, BlockPos pos) {
        int x = pos.X, y = pos.Y, z = pos.Z;
        if (!(level.IsLit(x, y, z) ^ layer != 1)) {
            int tex = GetTexture(TexId);
            float u0 = (float)(tex % 16) / 16.0F;
            float u1 = u0 + 0.0624375F;
            float v0 = (float)(tex / 16) / 16.0F;
            float v1 = v0 + 0.0624375F;
            int rots = 2;
            t.Color(1.0F, 1.0F, 1.0F);

            for(int r = 0; r < rots; ++r) {
                float xa = (float)(Math.Sin((double)r * Math.PI / (double)rots + (Math.PI / 4D)) * (double)0.5F);
                float za = (float)(Math.Cos((double)r * Math.PI / (double)rots + (Math.PI / 4D)) * (double)0.5F);
                float x0 = (float)x + 0.5F - xa;
                float x1 = (float)x + 0.5F + xa;
                float y0 = (float)y + 0.0F;
                float y1 = (float)y + 1.0F;
                float z0 = (float)z + 0.5F - za;
                float z1 = (float)z + 0.5F + za;
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