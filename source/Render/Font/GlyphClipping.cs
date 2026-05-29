using System.Collections.Generic;
using Godot;

namespace TestClient.Source.Render.Font;

public static class GlyphClipping
{
    public const int AsciiTexSize = 128;
    public const int AsciiCellW = 8;
    public const int AsciiCellH = 8;
    public const int AsciiGridCols = 16;
    public const int AsciiGridRows = 16;
    public const int AsciiCharCount = 256;

    public const int UnicodeTexSize = 256;
    public const int UnicodeCellW = 16;
    public const int UnicodeCellH = 16;
    public const int UnicodeGridCols = 16;
    public const int UnicodeGridRows = 16;
    public const int CharsPerPage = 256;

    private static ClippedGlyph[] _asciiGlyphs;
    private static readonly Dictionary<int, ClippedGlyph[]> _unicodePages = new(32);

    public static bool IsInitialized { get; private set; }

    public static void Initialize()
    {
        if (IsInitialized)
            return;

        ScanAsciiTexture();
        IsInitialized = true;
    }

    public static void Reload()
    {
        _asciiGlyphs = null;
        _unicodePages.Clear();
        IsInitialized = false;
        Initialize();
    }

    public static void Clear()
    {
        _asciiGlyphs = null;
        _unicodePages.Clear();
        IsInitialized = false;
    }

    private static void ScanAsciiTexture()
    {
        var tex = FontTexture.GetAsciiTexture();
        if (tex == null)
        {
            GD.PushError("[GlyphClipping] ASCII texture not loaded, using fallback widths");
            _asciiGlyphs = BuildFallbackAscii();
            return;
        }

        var image = tex.GetImage();
        if (image == null || image.IsEmpty())
        {
            GD.PushError("[GlyphClipping] ASCII texture image is empty, using fallback");
            _asciiGlyphs = BuildFallbackAscii();
            return;
        }

        if (image.GetFormat() != Image.Format.Rgba8)
            image.Convert(Image.Format.Rgba8);

        var texW = image.GetWidth();
        var texH = image.GetHeight();

        _asciiGlyphs = new ClippedGlyph[AsciiCharCount];

        for (var idx = 0; idx < AsciiCharCount; idx++)
        {
            var col = idx % AsciiGridCols;
            var row = idx / AsciiGridCols;

            // Handle space character specially
            if (idx == 32)
            {
                float spaceWidth = GlyphSizes.GetWidthAscii(32);
                if (spaceWidth <= 0f)
                {
                    var f = texW / 128.0f;
                    spaceWidth = AsciiCellW <= 8 ? 2.0f * f : 1.5f * f;
                }

                if (spaceWidth < 2f) spaceWidth = 2f;

                _asciiGlyphs[idx] = new ClippedGlyph
                {
                    U0 = (float)col * AsciiCellW / texW,
                    V0 = (float)row * AsciiCellH / texH,
                    U1 = (float)((col + 1) * AsciiCellW) / texW,
                    V1 = (float)((row + 1) * AsciiCellH) / texH,
                    Width = spaceWidth,
                    FullWidth = AsciiCellW,
                    Height = AsciiCellH,
                    HasContent = false
                };
                continue;
            }

            // Find bounding box of the character
            var bounds = FindCharacterBounds(image, col, row, AsciiCellW, AsciiCellH, texW, texH);
            
            var hasContent = bounds.Rightmost >= bounds.Leftmost && bounds.Bottommost >= bounds.Topmost;

            if (!hasContent)
            {
                _asciiGlyphs[idx] = new ClippedGlyph
                {
                    U0 = (float)col * AsciiCellW / texW,
                    V0 = (float)row * AsciiCellH / texH,
                    U1 = (float)((col + 1) * AsciiCellW) / texW,
                    V1 = (float)((row + 1) * AsciiCellH) / texH,
                    Width = 0f,
                    FullWidth = AsciiCellW,
                    Height = AsciiCellH,
                    HasContent = false
                };
                continue;
            }

            var scale = texW / 128.0f;
            var u0 = (float)(bounds.OriginX + bounds.Leftmost) / texW;
            var v0 = (float)(bounds.OriginY + 0) / texH;
            var u1 = (float)(bounds.OriginX + bounds.Rightmost + 1) / texW;
            var v1 = (float)(bounds.OriginY + bounds.Bottommost + 1) / texH;
            var renderWidth = bounds.Rightmost - bounds.Leftmost + 1 + 1.0f;

            _asciiGlyphs[idx] = new ClippedGlyph
            {
                U0 = u0,
                V0 = v0,
                U1 = u1,
                V1 = v1,
                Width = renderWidth,
                FullWidth = AsciiCellW,
                Height = AsciiCellH,
                HasContent = true
            };
        }
    }

    public static void ScanUnicodePage(int page)
    {
        if (_unicodePages.ContainsKey(page))
            return;

        var tex = FontTexture.GetPageTexture(page);
        if (tex == null)
            return;

        var image = tex.GetImage();
        if (image == null || image.IsEmpty())
            return;

        if (image.GetFormat() != Image.Format.Rgba8)
            image.Convert(Image.Format.Rgba8);

        var texW = image.GetWidth();
        var texH = image.GetHeight();

        var glyphs = new ClippedGlyph[CharsPerPage];

        for (var cellIdx = 0; cellIdx < CharsPerPage; cellIdx++)
        {
            var col = cellIdx % UnicodeGridCols;
            var row = cellIdx / UnicodeGridCols;

            // Find bounding box of the character
            var bounds = FindCharacterBounds(image, col, row, UnicodeCellW, UnicodeCellH, texW, texH);
            
            var hasContent = bounds.Rightmost >= bounds.Leftmost && bounds.Bottommost >= bounds.Topmost;

            if (!hasContent)
            {
                glyphs[cellIdx] = new ClippedGlyph
                {
                    U0 = (float)bounds.OriginX / texW,
                    V0 = (float)bounds.OriginY / texH,
                    U1 = (float)(bounds.OriginX + UnicodeCellW) / texW,
                    V1 = (float)(bounds.OriginY + UnicodeCellH) / texH,
                    Width = 0f,
                    FullWidth = UnicodeCellW,
                    Height = UnicodeCellH,
                    HasContent = false
                };
                continue;
            }

            var u0 = (float)(bounds.OriginX + bounds.Leftmost) / texW;
            var v0 = (float)(bounds.OriginY + 0) / texH;
            var u1 = (float)(bounds.OriginX + bounds.Rightmost + 1) / texW;
            var v1 = (float)(bounds.OriginY + bounds.Bottommost + 1) / texH;

            var renderWidth = bounds.Rightmost - bounds.Leftmost + 1 + 1.0f;

            glyphs[cellIdx] = new ClippedGlyph
            {
                U0 = u0,
                V0 = v0,
                U1 = u1,
                V1 = v1,
                Width = renderWidth,
                FullWidth = UnicodeCellW,
                Height = UnicodeCellH,
                HasContent = true
            };
        }

        _unicodePages[page] = glyphs;
    }

    public static bool GetAsciiClipping(int codepoint, out ClippedGlyph glyph)
    {
        if ((uint)codepoint < 128u && _asciiGlyphs != null)
        {
            glyph = _asciiGlyphs[codepoint];
            return true;
        }

        glyph = default;
        return false;
    }

    public static bool GetUnicodeClipping(int codepoint, out ClippedGlyph glyph)
    {
        if ((uint)codepoint < 128u || (uint)codepoint >= 65536u)
        {
            glyph = default;
            return false;
        }

        var page = codepoint >> 8;

        if (!_unicodePages.TryGetValue(page, out var pageGlyphs))
        {
            if (!FontTexture.PageExists(page))
            {
                glyph = default;
                return false;
            }

            ScanUnicodePage(page);
            if (!_unicodePages.TryGetValue(page, out pageGlyphs))
            {
                glyph = default;
                return false;
            }
        }

        var cellIdx = codepoint & 0xFF;
        glyph = pageGlyphs[cellIdx];
        return true;
    }

    public static bool GetBestClipping(int codepoint, out Texture2D texture, out ClippedGlyph glyph)
    {
        if ((uint)codepoint < 128u && _asciiGlyphs != null)
        {
            texture = FontTexture.GetAsciiTexture();
            glyph = _asciiGlyphs[codepoint];
            return true;
        }

        if ((uint)codepoint < 65536u)
        {
            var page = codepoint >> 8;
            if (!_unicodePages.TryGetValue(page, out var pageGlyphs))
            {
                if (!FontTexture.PageExists(page))
                    goto fallback;
                ScanUnicodePage(page);
                if (!_unicodePages.TryGetValue(page, out pageGlyphs))
                    goto fallback;
            }

            texture = FontTexture.GetPageTexture(page);
            glyph = pageGlyphs[codepoint & 0xFF];
            return texture != null;
        }

        fallback:
        texture = FontTexture.GetAsciiTexture();
        glyph = default;
        return false;
    }

    public static float GetCharWidth(int codepoint)
    {
        if ((uint)codepoint < 128u && _asciiGlyphs != null)
            return _asciiGlyphs[codepoint].Width;

        if ((uint)codepoint >= 65536u)
            return 0f;

        var page = codepoint >> 8;
        if (_unicodePages.TryGetValue(page, out var pageGlyphs))
            return pageGlyphs[codepoint & 0xFF].Width;
        if (FontTexture.PageExists(page))
        {
            ScanUnicodePage(page);
            if (_unicodePages.TryGetValue(page, out pageGlyphs))
                return pageGlyphs[codepoint & 0xFF].Width;
        }

        return 0f;
    }

    private static bool PixelVisible(Image img, int x, int y, int w, int h)
    {
        if ((uint)x >= (uint)w || (uint)y >= (uint)h)
            return false;

        var c = img.GetPixel(x, y);
        return c.A > 16f / 255f;
    }

    private static ClippedGlyph[] BuildFallbackAscii()
    {
        var glyphs = new ClippedGlyph[AsciiCharCount];
        for (var i = 0; i < AsciiCharCount; i++)
        {
            var col = i % AsciiGridCols;
            var row = i / AsciiGridCols;

            if (i == 32) // Space
            {
                glyphs[i] = new ClippedGlyph
                {
                    U0 = (float)col * AsciiCellW / AsciiTexSize,
                    V0 = (float)row * AsciiCellH / AsciiTexSize,
                    U1 = (float)((col + 1) * AsciiCellW) / AsciiTexSize,
                    V1 = (float)((row + 1) * AsciiCellH) / AsciiTexSize,
                    Width = 4f,
                    FullWidth = AsciiCellW,
                    Height = AsciiCellH,
                    HasContent = false
                };
                continue;
            }

            glyphs[i] = new ClippedGlyph
            {
                U0 = (float)col * AsciiCellW / AsciiTexSize,
                V0 = (float)row * AsciiCellH / AsciiTexSize,
                U1 = (float)((col + 1) * AsciiCellW) / AsciiTexSize,
                V1 = (float)((row + 1) * AsciiCellH) / AsciiTexSize,
                Width = AsciiCellW,
                FullWidth = AsciiCellW,
                Height = AsciiCellH,
                HasContent = true
            };
        }

        return glyphs;
    }
    
    private static (int Leftmost, int Topmost, int Rightmost, int Bottommost, int OriginX, int OriginY) FindCharacterBounds(
        Image image, int col, int row, int cellW, int cellH, int texW, int texH)
    {
        var originX = col * cellW;
        var originY = row * cellH;

        // Find leftmost pixel
        int leftmost = cellW;
        for (var px = 0; px < cellW && leftmost == cellW; px++)
        {
            for (var py = 0; py < cellH; py++)
            {
                if (PixelVisible(image, originX + px, originY + py, texW, texH))
                {
                    leftmost = px;
                    break;
                }
            }
        }

        // Find topmost pixel
        int topmost = cellH;
        for (var py = 0; py < cellH && topmost == cellH; py++)
        {
            for (var px = leftmost; px < cellW; px++)
            {
                if (PixelVisible(image, originX + px, originY + py, texW, texH))
                {
                    topmost = py;
                    break;
                }
            }
        }

        // Find rightmost pixel
        int rightmost = -1;
        for (var px = cellW - 1; px >= leftmost; px--)
        {
            for (var py = topmost; py < cellH; py++)
            {
                if (PixelVisible(image, originX + px, originY + py, texW, texH))
                {
                    rightmost = px;
                    break;
                }
            }
            if (rightmost >= 0) break;
        }

        // Find bottommost pixel
        int bottommost = -1;
        for (var py = cellH - 1; py >= topmost; py--)
        {
            for (var px = leftmost; px <= rightmost; px++)
            {
                if (PixelVisible(image, originX + px, originY + py, texW, texH))
                {
                    bottommost = py;
                    break;
                }
            }
            if (bottommost >= 0) break;
        }

        return (leftmost, topmost, rightmost, bottommost, originX, originY);
    }

    public struct ClippedGlyph
    {
        public float U0;
        public float V0;
        public float U1;
        public float V1;
        public float Width;
        public float FullWidth;
        public float Height;
        public bool HasContent;
    }
}