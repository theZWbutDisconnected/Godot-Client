using Godot;
using System;

namespace TestClient.Source.Render;

public static class ColorMap
{
    private static int[] _grassBuffer = new int[65536];
    private static int[] _foliageBuffer = new int[65536];
    
    static ColorMap()
    {
        LoadColorMaps();
    }
    
    private static void LoadColorMaps()
    {
        var grassImg = new Image();
        if (grassImg.Load("assets/colormap/grass.png") == Error.Ok)
        {
            LoadColorMap(grassImg, _grassBuffer);
        }
        
        var foliageImg = new Image();
        if (foliageImg.Load("assets/colormap/foliage.png") == Error.Ok)
        {
            LoadColorMap(foliageImg, _foliageBuffer);
        }
    }
    
    private static void LoadColorMap(Image img, int[] buffer)
    {
        int width = img.GetWidth();
        int height = img.GetHeight();
        
        for (int y = 0; y < height && y < 256; y++)
        {
            for (int x = 0; x < width && x < 256; x++)
            {
                var color = img.GetPixel(x, y);
                int rgb = (int)(color.R * 255) << 16 | (int)(color.G * 255) << 8 | (int)(color.B * 255);
                buffer[y << 8 | x] = rgb;
            }
        }
    }
    
    public static int GetGrassColor(double temperature, double rainfall)
    {
        rainfall *= temperature;
        
        int i = (int)((1.0 - temperature) * 255.0);
        int j = (int)((1.0 - rainfall) * 255.0);
        int k = j << 8 | i;
        
        return k >= _grassBuffer.Length ? 0x7fb238 : _grassBuffer[k];
    }
    
    public static int GetFoliageColor(double temperature, double rainfall)
    {
        rainfall *= temperature;
        
        int i = (int)((1.0 - temperature) * 255.0);
        int j = (int)((1.0 - rainfall) * 255.0);
        int k = j << 8 | i;
        
        return k >= _foliageBuffer.Length ? 0x5e8c29 : _foliageBuffer[k];
    }
}