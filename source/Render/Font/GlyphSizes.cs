using System;
using System.IO;
using Godot;

namespace TestClient.Source.Render.Font;

public static class GlyphSizes
{
    private const byte DefaultWidth = 8;
    private const byte NoGlyph = 0xFF;

    private static readonly byte[] _widths;
    private static readonly byte[] _asciiDefaultWidths;

    static GlyphSizes()
    {
        _widths = new byte[65536];

        _asciiDefaultWidths = new byte[128];
        for (var i = 0; i < 128; i++) _asciiDefaultWidths[i] = i == 0x20 ? (byte)4 : DefaultWidth;

        LoadGlyphSizes();
    }

    private static void LoadGlyphSizes()
    {
        var path = "assets/font/glyph_sizes.bin";
        try
        {
            if (!File.Exists(path))
            {
                GD.PushWarning($"[GlyphSizes] glyph_sizes.bin not found at '{path}', using defaults");
                Array.Fill(_widths, NoGlyph);
                return;
            }

            var bytes = File.ReadAllBytes(path);
            if (bytes.Length != 65536)
            {
                GD.PushWarning($"[GlyphSizes] Expected 65536 bytes, got {bytes.Length}, using defaults");
                Array.Fill(_widths, NoGlyph);
                return;
            }

            Buffer.BlockCopy(bytes, 0, _widths, 0, 65536);
        }
        catch (Exception e)
        {
            GD.PushError($"[GlyphSizes] Failed to load: {e.Message}");
            Array.Fill(_widths, NoGlyph);
        }
    }

    public static int GetWidth(int codepoint)
    {
        if ((uint)codepoint >= 65536u)
            return 0;

        var w = _widths[codepoint];
        if (w == NoGlyph)
            return 0;

        return w;
    }

    public static int GetWidthAscii(int codepoint)
    {
        if ((uint)codepoint >= 128u)
            return GetWidth(codepoint);

        return _asciiDefaultWidths[codepoint];
    }

    public static int GetWidthOrDefault(int codepoint)
    {
        if ((uint)codepoint >= 65536u)
            return 0;

        var w = _widths[codepoint];
        if (w != NoGlyph)
            return w;

        if (codepoint < 128)
            return _asciiDefaultWidths[codepoint];

        return 0;
    }

    public static bool HasGlyph(int codepoint)
    {
        if ((uint)codepoint >= 65536u)
            return false;

        return _widths[codepoint] != NoGlyph;
    }

    public static byte RawWidth(int codepoint)
    {
        return _widths[codepoint];
    }
}