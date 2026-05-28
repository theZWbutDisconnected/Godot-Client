﻿using Godot;
using TestClient.Source.Physics;
using TestClient.Source.Render;
using TestClient.Source.World.Biome;

namespace TestClient.Source.World.Tile;

public class GrassBlock : Block
{
    private static readonly int GrassSideOverlayTex = TextureAtlas.Index("grass_side_overlay");

    public GrassBlock(int id) : base(id, TextureAtlas.Index("dirt"))
    {
    }

    protected override int GetTexture(int downMeta, int face)
    {
        if (face == 1) return TextureAtlas.Index("grass_top");
        return face == 0 ? TextureAtlas.Index("dirt") : TextureAtlas.Index("grass_side");
    }

    public override void Render(Tessellator t, Level level, BlockPos pos)
    {
        float c1;
        float c2;
        float c3;
        int x = pos.X, y = pos.Y, z = pos.Z;

        var grassColorInt = BiomeColorHelper.GetGrassColorAtPos(level, pos);
        var gcR = ((grassColorInt >> 16) & 0xFF) / 255f;
        var gcG = ((grassColorInt >> 8) & 0xFF) / 255f;
        var gcB = (grassColorInt & 0xFF) / 255f;

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
            t.Color(gcR * c1, gcG * c1, gcB * c1);
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

            t.Color(gcR * c2, gcG * c2, gcB * c2);
            RenderFaceWithTex(t, level, x, y, z, 2, GrassSideOverlayTex);
        }

        if (ShouldRenderFace(level, new BlockPos(x, y, z + 1)))
        {
            c2 = 0.8F;
            if (!level.IsLit(x, y, z + 1))
                c2 *= 0.5F;

            t.Color(c2, c2, c2);
            t.Normal(0, 0, 1);
            RenderFace(t, level, x, y, z, 3);

            t.Color(gcR * c2, gcG * c2, gcB * c2);
            RenderFaceWithTex(t, level, x, y, z, 3, GrassSideOverlayTex);
        }

        if (ShouldRenderFace(level, new BlockPos(x - 1, y, z)))
        {
            c3 = 0.6F;
            if (!level.IsLit(x - 1, y, z))
                c3 *= 0.5F;

            t.Color(c3, c3, c3);
            t.Normal(-1, 0, 0);
            RenderFace(t, level, x, y, z, 4);

            t.Color(gcR * c3, gcG * c3, gcB * c3);
            RenderFaceWithTex(t, level, x, y, z, 4, GrassSideOverlayTex);
        }

        if (ShouldRenderFace(level, new BlockPos(x + 1, y, z)))
        {
            c3 = 0.6F;
            if (!level.IsLit(x + 1, y, z))
                c3 *= 0.5F;

            t.Color(c3, c3, c3);
            t.Normal(1, 0, 0);
            RenderFace(t, level, x, y, z, 5);

            t.Color(gcR * c3, gcG * c3, gcB * c3);
            RenderFaceWithTex(t, level, x, y, z, 5, GrassSideOverlayTex);
        }
    }
}