using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace TestClient.Source.Render;

public static class TextureAtlas
{
    public const int TileSize = 16;
    public static readonly int AtlasWidth;
    public static readonly float TileUVSize;
    public static readonly ImageTexture AtlasTexture;

    private static readonly Dictionary<string, int> NameToIndex;

    static TextureAtlas()
    {
        var pngFiles = Directory.GetFiles("assets/blocks/", "*.png")
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        int count = pngFiles.Length;
        AtlasWidth = (int)Math.Ceiling(Math.Sqrt(count));
        TileUVSize = 1.0f / AtlasWidth;

        NameToIndex = new Dictionary<string, int>(count);
        for (int i = 0; i < count; i++)
            NameToIndex[pngFiles[i]] = i;

        int atlasPixelSize = AtlasWidth * TileSize;
        var atlasImage = Image.Create(atlasPixelSize, atlasPixelSize, false, Image.Format.Rgba8);

        for (int i = 0; i < count; i++)
        {
            var texImage = new Image();
            var path = $"assets/blocks/{pngFiles[i]}.png";
            if (texImage.Load(path) != Error.Ok)
            {
                GD.PushWarning($"[TextureAtlas] Failed to load: {path}");
                continue;
            }

            texImage.Resize(TileSize, TileSize, Image.Interpolation.Nearest);

            int col = i % AtlasWidth;
            int row = i / AtlasWidth;
            atlasImage.BlitRect(texImage,
                new Rect2I(0, 0, TileSize, TileSize),
                new Vector2I(col * TileSize, row * TileSize));
        }

        AtlasTexture = ImageTexture.CreateFromImage(atlasImage);
    }

    public static int Index(string name)
    {
        return NameToIndex.TryGetValue(name, out var idx) ? idx : 0;
    }

    public static void GetUV(int texIndex, out float u0, out float v0, out float u1, out float v1)
    {
        float tileUV = TileUVSize;

        int col = texIndex % AtlasWidth;
        int row = texIndex / AtlasWidth;

        u0 = col * tileUV;
        v0 = row * tileUV;
        u1 = (col + 1) * tileUV;
        v1 = (row + 1) * tileUV;
    }
}
