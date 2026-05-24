using System;
using TestClient.Source.Physics;
using TestClient.Source.Render;

namespace TestClient.Source.World.Biome;

public class Biome
{
    public static readonly Biome Default = new(0)
    {
        Temperature = 0.5f,
        Rainfall = 0.5f,
        GrassColor = 0x7fb238,
        FoliageColor = 0x5e8c29,
        WaterColor = 0x3f76e4
    };

    public readonly int Id;
    public int Color = 0;
    public int FoliageColor;
    public int GrassColor;
    public float Rainfall = 0.5f;
    public float Temperature = 0.5f;
    public int WaterColor = 0x3f76e4;

    public Biome(int id)
    {
        Id = id;
    }

    public virtual int GetGrassColorAtPos(BlockPos pos)
    {
        var temp = Math.Clamp(GetFloatTemperature(pos), 0.0, 1.0);
        var rain = Math.Clamp(GetFloatRainfall(), 0.0, 1.0);
        return ColorMap.GetGrassColor(temp, rain);
    }

    public virtual int GetFoliageColorAtPos(BlockPos pos)
    {
        var temp = Math.Clamp(GetFloatTemperature(pos), 0.0, 1.0);
        var rain = Math.Clamp(GetFloatRainfall(), 0.0, 1.0);
        return ColorMap.GetFoliageColor(temp, rain);
    }

    public virtual float GetFloatTemperature(BlockPos pos)
    {
        return Temperature;
    }

    public virtual float GetFloatRainfall()
    {
        return Rainfall;
    }
}