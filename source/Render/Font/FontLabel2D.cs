using System.Collections.Generic;
using Godot;

namespace TestClient.Source.Render.Font;

public partial class FontLabel2D : Control
{
	private const float GlyphH = 16f;
	private const float AsciiCellW = 8f;
	private const float UnicodeCellW = 16f;
	private const float ShadowOff = 1f;
	private const float BoldOff = 1f;
	private const char Sec = '\u00A7';

	private static readonly Color[] McColors =
	{
		new(0f, 0f, 0f), new(0f, 0f, 0.67f), new(0f, 0.67f, 0f), new(0f, 0.67f, 0.67f),
		new(0.67f, 0f, 0f), new(0.67f, 0f, 0.67f), new(1f, 0.67f, 0f), new(0.67f, 0.67f, 0.67f),
		new(0.33f, 0.33f, 0.33f), new(0.33f, 0.33f, 1f), new(0.33f, 1f, 0.33f), new(0.33f, 1f, 1f),
		new(1f, 0.33f, 0.33f), new(1f, 0.33f, 1f), new(1f, 1f, 0.33f), new(1f, 1f, 1f)
	};

	private static Image _asciiImg;
	private static readonly Dictionary<int, Image> _pageImgs = new();
	private readonly List<Glyph> _glyphs = new();
	private ImageTexture _cachedTex;
	private bool _dirty = true;

	private string _raw;
	private FontStyle _style;

	public float TextWidth { get; private set; }

	public void SetText(string text, FontStyle? style = null)
	{
		var s = style ?? _style;
		if (_raw == text && _style.Color == s.Color && _style.Shadow == s.Shadow
			&& _style.Bold == s.Bold) return;
		_raw = text;
		_style = s;
		_dirty = true;
		QueueRedraw();
	}

	public override void _Draw()
	{
		if (_dirty)
		{
			Parse();
			_cachedTex = null;
			_dirty = false;
		}

		if (_glyphs.Count == 0) return;

		if (_cachedTex == null)
			_cachedTex = BuildTexture();

		if (_cachedTex != null)
			DrawTexture(_cachedTex, Vector2.Zero);
	}

	private ImageTexture BuildTexture()
	{
		if (_glyphs.Count == 0) return null;

		var w = Mathf.CeilToInt(TextWidth);
		var h = (int)GlyphH;
		if (_glyphs.Count > 1)
		{
			float maxY = 0;
			foreach (var g in _glyphs)
				if (g.Y + GlyphH > maxY)
					maxY = g.Y + GlyphH;
			h = Mathf.CeilToInt(maxY);
		}

		var outImg = Image.Create(w, h, false, Image.Format.Rgba8);
		outImg.Fill(new Color(0, 0, 0, 0));

		foreach (var g in _glyphs)
		{
			var srcImg = GetSourceImage(g.Page);
			if (srcImg == null) continue;

			var cellWidth = g.Page == -1 ? 8 : 16;
			var cellHeight = g.Page == -1 ? 8 : 16;
			var asciiClipped = new GlyphClipping.ClippedGlyph();
			var unicodeClipped = new GlyphClipping.ClippedGlyph();
			bool hasClipping = g.Page == -1 ? 
				GlyphClipping.GetAsciiClipping(g.Cp, out asciiClipped) && asciiClipped.HasContent :
				GlyphClipping.GetUnicodeClipping(g.Cp, out unicodeClipped) && unicodeClipped.HasContent;

			int srcX, srcY, copyW, copyH;
			if (hasClipping)
			{
				var clipped = g.Page == -1 ? asciiClipped : unicodeClipped;
				srcX = Mathf.RoundToInt(clipped.U0 * srcImg.GetWidth());
				srcY = Mathf.RoundToInt(clipped.V0 * srcImg.GetHeight());
				copyW = Mathf.RoundToInt((clipped.U1 - clipped.U0) * srcImg.GetWidth());
				copyH = Mathf.RoundToInt((clipped.V1 - clipped.V0) * srcImg.GetHeight());
			}
			else
			{
				ComputeCellPos(g.Cp, g.Page, out var col, out var row);
				srcX = col * cellWidth;
				srcY = row * cellHeight;
				copyW = cellWidth;
				copyH = cellHeight;
			}

			if (copyW <= 0 || copyH <= 0) continue;

			var dstX = Mathf.RoundToInt(g.X);
			var dstY = Mathf.RoundToInt(g.Y);

			float cr = g.Clr.R, cg = g.Clr.G, cb = g.Clr.B, ca = g.Clr.A;

			for (var py = 0; py < copyH; py++)
			{
				var sy = srcY + py;
				var dy = dstY + py;
				if (sy >= srcImg.GetHeight() || dy >= h) continue;

				for (var px = 0; px < copyW; px++)
				{
					var sx = srcX + px;
					var dx = dstX + px;
					if (sx >= srcImg.GetWidth() || dx >= w) continue;

					var sp = srcImg.GetPixel(sx, sy);
					if (sp.A <= 0) continue;

					outImg.SetPixel(dx, dy, new Color(cr, cg, cb, sp.A * ca));
				}
			}
		}

		return ImageTexture.CreateFromImage(outImg);
	}

	private static Image GetSourceImage(int page)
	{
		if (page == -1)
		{
			if (_asciiImg == null)
			{
				var texture1 = ResourceLoader.Load<Texture2D>("res://assets/font/ascii.png");
				if (texture1 == null)
					return null;
				_asciiImg = texture1.GetImage();
				if (_asciiImg.GetFormat() != Image.Format.Rgba8)
					_asciiImg.Convert(Image.Format.Rgba8);
			}

			return _asciiImg;
		}

		if (_pageImgs.TryGetValue(page, out var img))
			return img;

		var texture = ResourceLoader.Load<Texture2D>($"res://assets/font/unicode_page_{page:X2}.png");
		if (texture == null)
			return null;
		var raw = texture.GetImage();
		if (raw.GetFormat() != Image.Format.Rgba8)
			raw.Convert(Image.Format.Rgba8);

		_pageImgs[page] = raw;
		return raw;
	}

	private static void ComputeCellPos(int cp, int page, out int col, out int row)
	{
		if (page == -1)
		{
			col = cp & 0x0F;
			row = cp >> 4;
		}
		else
		{
			var cell = cp & 0xFF;
			col = cell & 0x0F;
			row = cell >> 4;
		}
	}

	private void Parse()
	{
		_glyphs.Clear();
		TextWidth = 0;
		Size = Vector2.Zero;

		if (string.IsNullOrEmpty(_raw)) return;

		if (!GlyphClipping.IsInitialized)
			GlyphClipping.Initialize();

		var color = _style.Color;
		bool bold = _style.Bold, italic = false, obf = false;
		float x = 0, y = 0;

		for (var i = 0; i < _raw.Length;)
		{
			var c = _raw[i];

			if (TryFmt(_raw, i, out var skip))
			{
				if (i + 1 < _raw.Length) ApplyFmt(_raw[i + 1], ref color, ref bold, ref italic, ref obf);
				i += skip;
				continue;
			}

			if (c == '\n')
			{
				x = 0;
				y += GlyphH;
				i++;
				continue;
			}

			int cp = c;
			if (char.IsHighSurrogate(c) && i + 1 < _raw.Length && char.IsLowSurrogate(_raw[i + 1]))
				cp = char.ConvertToUtf32(c, _raw[i += 2]);
			else i++;

			if (cp > 0xFFFF) continue;

			var w = GlyphClipping.GetCharWidth(cp);
			if (w <= 0f) w = cp < 128 ? AsciiCellW : UnicodeCellW;
			if (w < 2f && cp == 32) w = 4f;

			var page = cp < 128 ? -1 : cp >> 8;

			if (_style.Shadow)
				_glyphs.Add(new Glyph
				{
					Cp = cp, Page = page, X = x + ShadowOff, Y = y + ShadowOff, Clr = new Color(0.15f, 0.15f, 0.15f)
				});
			if (bold)
				_glyphs.Add(new Glyph { Cp = cp, Page = page, X = x + BoldOff, Y = y, Clr = color });
			_glyphs.Add(new Glyph { Cp = cp, Page = page, X = x, Y = y, Clr = color });

			x += w + (bold ? 1f : 0f);
		}

		TextWidth = x;
		Size = new Vector2(x, GlyphH);
	}

	private static bool TryFmt(string s, int i, out int skip)
	{
		skip = 0;
		if (i >= s.Length) return false;
		if (s[i] == Sec && i + 1 < s.Length)
		{
			skip = 2;
			return true;
		}

		if (s[i] == '&' && i + 1 < s.Length)
		{
			var n = s[i + 1];
			if ((n >= '0' && n <= '9') || (n >= 'a' && n <= 'f') || "lmnokr".Contains(char.ToLower(n)))
			{
				skip = 2;
				return true;
			}
		}

		return false;
	}

	private static void ApplyFmt(char code, ref Color c, ref bool b, ref bool i, ref bool o)
	{
		if (code >= '0' && code <= '9')
		{
			c = McColors[code - '0'];
			b = i = o = false;
		}
		else if (code >= 'a' && code <= 'f')
		{
			c = McColors[code - 'a' + 10];
			b = i = o = false;
		}
		else
		{
			switch (char.ToLower(code))
			{
				case 'l': b = true; break;
				case 'o': i = true; break;
				case 'k': o = true; break;
				case 'r':
					c = Colors.White;
					b = i = o = false;
					break;
			}
		}
	}

	public override void _ExitTree()
	{
		if (_cachedTex != null)
		{
			_cachedTex.Dispose();
			_cachedTex = null;
		}
	}

	public struct FontStyle
	{
		public Color Color;
		public bool Shadow, Bold;

		public static readonly FontStyle Default = new()
			{ Color = Colors.White, Shadow = true, Bold = false };

		public readonly FontStyle WithColor(Color c)
		{
			return new FontStyle
				{ Color = c, Shadow = Shadow, Bold = Bold };
		}

		public readonly FontStyle WithShadow(bool s)
		{
			return new FontStyle
				{ Color = Color, Shadow = s, Bold = Bold };
		}
	}

	private struct Glyph
	{
		public int Cp, Page;
		public float X, Y;
		public Color Clr;
	}
}