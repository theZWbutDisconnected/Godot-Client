using System.Collections.Generic;

namespace TestClient.Source.World.Biome;

public static class BiomeRegistry
{
    private static readonly Dictionary<int, Biome> _biomes = new Dictionary<int, Biome>();

    static BiomeRegistry()
    {
        RegisterDefaultBiomes();
    }

    private static void RegisterDefaultBiomes()
    {
        // Ocean
        RegisterBiome(new Biome(0)
        {
            Temperature = 0.5f,
            Rainfall = 0.5f,
            Color = 112,
            WaterColor = 0x3f76e4
        });

        // Plains
        RegisterBiome(new Biome(1)
        {
            Temperature = 0.8f,
            Rainfall = 0.4f,
            Color = 9286496,
            WaterColor = 0x3f76e4
        });

        // Desert
        RegisterBiome(new Biome(2)
        {
            Temperature = 2.0f,
            Rainfall = 0.0f,
            Color = 16421912,
            WaterColor = 0x3f76e4
        });

        // Extreme Hills
        RegisterBiome(new Biome(3)
        {
            Temperature = 0.2f,
            Rainfall = 0.3f,
            Color = 6316128,
            WaterColor = 0x3f76e4
        });

        // Forest
        RegisterBiome(new Biome(4)
        {
            Temperature = 0.7f,
            Rainfall = 0.8f,
            Color = 353825,
            WaterColor = 0x3f76e4
        });

        // Taiga
        RegisterBiome(new Biome(5)
        {
            Temperature = 0.25f,
            Rainfall = 0.8f,
            Color = 747097,
            WaterColor = 0x3f76e4
        });

        // Swampland
        RegisterBiome(new Biome(6)
        {
            Temperature = 0.8f,
            Rainfall = 0.9f,
            Color = 522674,
            WaterColor = 0x3f76e4
        });

        // River
        RegisterBiome(new Biome(7)
        {
            Temperature = 0.5f,
            Rainfall = 0.5f,
            Color = 255,
            WaterColor = 0x3f76e4
        });

        // Nether
        RegisterBiome(new Biome(8)
        {
            Temperature = 2.0f,
            Rainfall = 0.0f,
            Color = 16711680,
            WaterColor = 0x3f76e4
        });

        // The End
        RegisterBiome(new Biome(9)
        {
            Temperature = 0.5f,
            Rainfall = 0.5f,
            Color = 8421631,
            WaterColor = 0x3f76e4
        });

        // FrozenOcean
        RegisterBiome(new Biome(10)
        {
            Temperature = 0.0f,
            Rainfall = 0.5f,
            Color = 9474208,
            WaterColor = 0x3f76e4
        });

        // FrozenRiver
        RegisterBiome(new Biome(11)
        {
            Temperature = 0.0f,
            Rainfall = 0.5f,
            Color = 10526975,
            WaterColor = 0x3f76e4
        });

        // Ice Plains
        RegisterBiome(new Biome(12)
        {
            Temperature = 0.0f,
            Rainfall = 0.5f,
            Color = 16777215,
            WaterColor = 0x3f76e4
        });

        // Ice Mountains
        RegisterBiome(new Biome(13)
        {
            Temperature = 0.0f,
            Rainfall = 0.5f,
            Color = 10526880,
            WaterColor = 0x3f76e4
        });

        // MushroomIsland
        RegisterBiome(new Biome(14)
        {
            Temperature = 0.9f,
            Rainfall = 1.0f,
            Color = 16711935,
            GrassColor = 0xff00ff,
            FoliageColor = 0xff00ff,
            WaterColor = 0x3f76e4
        });

        // MushroomIslandShore
        RegisterBiome(new Biome(15)
        {
            Temperature = 0.9f,
            Rainfall = 1.0f,
            Color = 10486015,
            WaterColor = 0x3f76e4
        });

        // Beach
        RegisterBiome(new Biome(16)
        {
            Temperature = 0.8f,
            Rainfall = 0.4f,
            Color = 16440917,
            WaterColor = 0x3f76e4
        });

        // DesertHills
        RegisterBiome(new Biome(17)
        {
            Temperature = 2.0f,
            Rainfall = 0.0f,
            Color = 13786898,
            WaterColor = 0x3f76e4
        });

        // ForestHills
        RegisterBiome(new Biome(18)
        {
            Temperature = 0.7f,
            Rainfall = 0.8f,
            Color = 2250012,
            WaterColor = 0x3f76e4
        });

        // TaigaHills
        RegisterBiome(new Biome(19)
        {
            Temperature = 0.25f,
            Rainfall = 0.8f,
            Color = 1456435,
            WaterColor = 0x3f76e4
        });

        // ExtremeHillsEdge
        RegisterBiome(new Biome(20)
        {
            Temperature = 0.2f,
            Rainfall = 0.3f,
            Color = 7501978,
            WaterColor = 0x3f76e4
        });

        // Jungle
        RegisterBiome(new Biome(21)
        {
            Temperature = 0.95f,
            Rainfall = 0.9f,
            Color = 5470985,
            WaterColor = 0x3f76e4
        });

        // JungleHills
        RegisterBiome(new Biome(22)
        {
            Temperature = 0.95f,
            Rainfall = 0.9f,
            Color = 2900485,
            WaterColor = 0x3f76e4
        });

        // JungleEdge
        RegisterBiome(new Biome(23)
        {
            Temperature = 0.95f,
            Rainfall = 0.8f,
            Color = 6458135,
            WaterColor = 0x3f76e4
        });

        // DeepOcean
        RegisterBiome(new Biome(24)
        {
            Temperature = 0.5f,
            Rainfall = 0.5f,
            Color = 48,
            WaterColor = 0x3f76e4
        });

        // StoneBeach
        RegisterBiome(new Biome(25)
        {
            Temperature = 0.2f,
            Rainfall = 0.3f,
            Color = 10658436,
            WaterColor = 0x3f76e4
        });

        // ColdBeach
        RegisterBiome(new Biome(26)
        {
            Temperature = 0.05f,
            Rainfall = 0.3f,
            Color = 16445632,
            WaterColor = 0x3f76e4
        });

        // BirchForest
        RegisterBiome(new Biome(27)
        {
            Temperature = 0.6f,
            Rainfall = 0.7f,
            Color = 3175492,
            WaterColor = 0x3f76e4
        });

        // BirchForestHills
        RegisterBiome(new Biome(28)
        {
            Temperature = 0.6f,
            Rainfall = 0.7f,
            Color = 2055986,
            WaterColor = 0x3f76e4
        });

        // RoofedForest
        RegisterBiome(new Biome(29)
        {
            Temperature = 0.7f,
            Rainfall = 0.8f,
            Color = 4215066,
            WaterColor = 0x3f76e4
        });

        // ColdTaiga
        RegisterBiome(new Biome(30)
        {
            Temperature = -0.5f,
            Rainfall = 0.4f,
            Color = 3233098,
            WaterColor = 0x3f76e4
        });

        // ColdTaigaHills
        RegisterBiome(new Biome(31)
        {
            Temperature = -0.5f,
            Rainfall = 0.4f,
            Color = 2375478,
            WaterColor = 0x3f76e4
        });

        // MegaTaiga
        RegisterBiome(new Biome(32)
        {
            Temperature = 0.3f,
            Rainfall = 0.8f,
            Color = 5858897,
            WaterColor = 0x3f76e4
        });

        // MegaTaigaHills
        RegisterBiome(new Biome(33)
        {
            Temperature = 0.3f,
            Rainfall = 0.8f,
            Color = 4542270,
            WaterColor = 0x3f76e4
        });

        // ExtremeHillsPlus
        RegisterBiome(new Biome(34)
        {
            Temperature = 0.2f,
            Rainfall = 0.3f,
            Color = 5271632,
            WaterColor = 0x3f76e4
        });

        // Savanna
        RegisterBiome(new Biome(35)
        {
            Temperature = 1.2f,
            Rainfall = 0.0f,
            Color = 12431967,
            WaterColor = 0x3f76e4
        });

        // SavannaPlateau
        RegisterBiome(new Biome(36)
        {
            Temperature = 1.0f,
            Rainfall = 0.0f,
            Color = 10984804,
            WaterColor = 0x3f76e4
        });

        // Mesa
        RegisterBiome(new Biome(37)
        {
            Temperature = 2.0f,
            Rainfall = 0.0f,
            Color = 14238997,
            WaterColor = 0x3f76e4
        });

        // MesaPlateau_F
        RegisterBiome(new Biome(38)
        {
            Temperature = 2.0f,
            Rainfall = 0.0f,
            Color = 11573093,
            WaterColor = 0x3f76e4
        });

        // MesaPlateau
        RegisterBiome(new Biome(39)
        {
            Temperature = 2.0f,
            Rainfall = 0.0f,
            Color = 13274213,
            WaterColor = 0x3f76e4
        });
    }

    public static void RegisterBiome(Biome biome)
    {
        _biomes[biome.Id] = biome;
    }

    public static Biome GetBiome(int id)
    {
        return _biomes.TryGetValue(id, out var biome) ? biome : Biome.Default;
    }
}