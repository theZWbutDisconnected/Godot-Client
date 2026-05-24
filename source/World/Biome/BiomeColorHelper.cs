using TestClient.Source.Physics;
using TestClient.Source.Render;

namespace TestClient.Source.World.Biome;

public static class BiomeColorHelper
{
    public static int GetGrassColorAtPos(Level level, BlockPos pos)
    {
        return GetColorAtPos(level, pos, GRASS_COLOR_RESOLVER);
    }

    public static int GetFoliageColorAtPos(Level level, BlockPos pos)
    {
        return GetColorAtPos(level, pos, FOLIAGE_COLOR_RESOLVER);
    }

    public static int GetWaterColorAtPos(Level level, BlockPos pos)
    {
        return GetColorAtPos(level, pos, WATER_COLOR_RESOLVER);
    }

    private static int GetColorAtPos(Level level, BlockPos pos, ColorResolver colorResolver)
    {
        int r = 0, g = 0, b = 0;

        for (int x = pos.X - 1; x <= pos.X + 1; x++)
        {
            for (int z = pos.Z - 1; z <= pos.Z + 1; z++)
            {
                var biome = GetBiomeAt(level, x, z);
                int color = colorResolver(biome, new BlockPos(x, pos.Y, z));
                r += (color >> 16) & 0xFF;
                g += (color >> 8) & 0xFF;
                b += color & 0xFF;
            }
        }

        return ((r / 9) << 16) | ((g / 9) << 8) | (b / 9);
    }

    public static Biome GetBiomeAt(Level level, int x, int z)
    {
        int chunkX = ChunkData.WorldToChunk(x);
        int chunkZ = ChunkData.WorldToChunk(z);

        var chunk = level.GetChunk(chunkX, chunkZ);
        if (chunk == null)
            return Biome.Default;

        int localX = ChunkData.WorldToLocal(x, chunkX);
        int localZ = ChunkData.WorldToLocal(z, chunkZ);

        int biomeIndex = localZ * 16 + localX;
        byte biomeId = chunk.GetBiomeArray()[biomeIndex];

        return BiomeRegistry.GetBiome(biomeId);
    }

    
    private static readonly ColorResolver GRASS_COLOR_RESOLVER = (biome, pos) =>
    {
        return biome.GetGrassColorAtPos(pos);
    };

    private static readonly ColorResolver FOLIAGE_COLOR_RESOLVER = (biome, pos) =>
    {
        return biome.GetFoliageColorAtPos(pos);
    };

    private static readonly ColorResolver WATER_COLOR_RESOLVER = (biome, pos) =>
    {
        return biome.WaterColor;
    };

    public delegate int ColorResolver(Biome biome, BlockPos pos);
}