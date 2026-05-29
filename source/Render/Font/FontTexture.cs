using System.Collections.Generic;
using Godot;

namespace TestClient.Source.Render.Font;

public static class FontTexture
{
	public const float GlyphTexW = 16f;
	public const float GlyphTexH = 16f;
	public const float PageSize = 256f;
	public const int GlyphsPerRow = 16;

	public const float AsciiPageSize = 128f;
	public const float AsciiGlyphW = 8f;
	public const float AsciiGlyphH = 8f;
	public const int AsciiGlyphsPerRow = 16;
	public const int AsciiMaxCodepoint = 127;

	private const int MaxCachedPages = 16;

	private static readonly LinkedList<int> _lruOrder = new();
	private static readonly Dictionary<int, LinkedListNode<int>> _lruNodes = new();
	private static readonly Dictionary<int, Texture2D> _pageCache = new(MaxCachedPages);

	private static Texture2D _asciiTexture;
	private static bool _asciiLoaded;

	private static readonly bool[] _availablePages = new bool[256];

	static FontTexture()
	{
		ScanAvailablePages();
	}

	private static void ScanAvailablePages()
	{
		for (var i = 0; i < 256; i++)
		{
			var path = $"res://assets/font/unicode_page_{i:X2}.png";
			_availablePages[i] = ResourceLoader.Exists(path);
		}
	}

	public static bool PageExists(int page)
	{
		return (uint)page < 256u && _availablePages[page];
	}

	public static Texture2D GetAsciiTexture()
	{
		if (!_asciiLoaded)
		{
			_asciiTexture = GD.Load<Texture2D>("res://assets/font/ascii.png");
			_asciiLoaded = true;
		}

		return _asciiTexture;
	}

	public static Texture2D GetPageTexture(int page)
	{
		if ((uint)page >= 256u || !_availablePages[page])
			return null;

		if (_pageCache.TryGetValue(page, out var cached))
		{
			var node = _lruNodes[page];
			if (node != _lruOrder.First)
			{
				_lruOrder.Remove(node);
				_lruOrder.AddFirst(node);
			}

			return cached;
		}

		var path = $"res://assets/font/unicode_page_{page:X2}.png";
		var texture = GD.Load<Texture2D>(path);
		if (texture == null)
			return null;

		if (_pageCache.Count >= MaxCachedPages)
		{
			var last = _lruOrder.Last;
			_pageCache.Remove(last.Value);
			_lruNodes.Remove(last.Value);
			_lruOrder.RemoveLast();
		}

		var newNode = _lruOrder.AddFirst(page);
		_lruNodes[page] = newNode;
		_pageCache[page] = texture;

		return texture;
	}

	public static bool GetAsciiUV(int codepoint, out float u0, out float v0, out float u1, out float v1)
	{
		if ((uint)codepoint > AsciiMaxCodepoint)
		{
			u0 = v0 = u1 = v1 = 0f;
			return false;
		}

		var col = codepoint % AsciiGlyphsPerRow;
		var row = codepoint / AsciiGlyphsPerRow;

		var texelU = 1f / AsciiPageSize;
		var texelV = 1f / AsciiPageSize;

		u0 = col * AsciiGlyphW * texelU;
		v0 = row * AsciiGlyphH * texelV;
		u1 = (col + 1) * AsciiGlyphW * texelU;
		v1 = (row + 1) * AsciiGlyphH * texelV;

		return true;
	}

	public static bool GetUnicodeUV(int codepoint, out float u0, out float v0, out float u1, out float v1)
	{
		if ((uint)codepoint >= 65536u)
		{
			u0 = v0 = u1 = v1 = 0f;
			return false;
		}

		var page = codepoint >> 8;
		if (!_availablePages[page])
		{
			u0 = v0 = u1 = v1 = 0f;
			return false;
		}

		var cell = codepoint & 0xFF;
		var col = cell % GlyphsPerRow;
		var row = cell / GlyphsPerRow;

		var texelU = 1f / PageSize;
		var texelV = 1f / PageSize;

		u0 = col * GlyphTexW * texelU;
		v0 = row * GlyphTexH * texelV;
		u1 = (col + 1) * GlyphTexW * texelU;
		v1 = (row + 1) * GlyphTexH * texelV;

		return true;
	}

	public static Texture2D GetBestTexture(int codepoint, out float u0, out float v0, out float u1, out float v1)
	{
		if (GetUnicodeUV(codepoint, out u0, out v0, out u1, out v1))
		{
			var page = codepoint >> 8;
			return GetPageTexture(page);
		}

		if (GetAsciiUV(codepoint, out u0, out v0, out u1, out v1)) return GetAsciiTexture();

		u0 = v0 = u1 = v1 = 0f;
		return GetAsciiTexture();
	}

	public static void PreloadPages(IEnumerable<int> pages)
	{
		foreach (var page in pages) GetPageTexture(page);
	}

	public static void ClearCache()
	{
		_lruOrder.Clear();
		_lruNodes.Clear();
		_pageCache.Clear();
		_asciiTexture = null;
		_asciiLoaded = false;
	}
}
