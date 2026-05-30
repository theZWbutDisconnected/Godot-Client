using System;
using System.Collections.Generic;
using Godot;
using TestClient.Source.Render.Font;

namespace TestClient.Source.Gui;

public partial class GuiRenderer : Control
{
	public static readonly Color[] McColors =
	{
		new(0f, 0f, 0f), new(0f, 0f, 0.67f), new(0f, 0.67f, 0f), new(0f, 0.67f, 0.67f),
		new(0.67f, 0f, 0f), new(0.67f, 0f, 0.67f), new(1f, 0.67f, 0f), new(0.67f, 0.67f, 0.67f),
		new(0.33f, 0.33f, 0.33f), new(0.33f, 0.33f, 1f), new(0.33f, 1f, 0.33f), new(0.33f, 1f, 1f),
		new(1f, 0.33f, 0.33f), new(1f, 0.33f, 1f), new(1f, 1f, 0.33f), new(1f, 1f, 1f)
	};

	private enum CmdType : byte { TexRect, Rect, GradientV, GradientH, Clip, Unclip }

	private struct Cmd
	{
		public CmdType Type;
		public float X, Y, W, H;
		public float U0, V0, U1, V1;
		public Color C0, C1;
		public Texture2D Tex;
	}

	private readonly List<Cmd> _cmds = new(256);
	private Texture2D _boundTex;
	private bool _dirty;
	
	public void Begin()
	{
		_cmds.Clear();
		_boundTex = null;
	}

	public void Bind(Texture2D texture) => _boundTex = texture;

	public void End()
	{
		_dirty = true;
		QueueRedraw();
	}

	public void DrawTexturedRect(float x, float y, float w, float h,
		float u0, float v0, float u1, float v1)
	{
		DrawTexturedRect(x, y, w, h, u0, v0, u1, v1, Colors.White);
	}
	
	public void DrawTexturedRect(float x, float y, float w, float h,
		float u0, float v0, float u1, float v1, Color color)
	{
		_cmds.Add(new Cmd
		{
			Type = CmdType.TexRect, X = x, Y = y, W = w, H = h,
			U0 = u0, V0 = v0, U1 = u1, V1 = v1, Tex = _boundTex, C0 = color
		});
	}

	public void DrawRect(float x, float y, float w, float h, Color color)
	{
		_cmds.Add(new Cmd { Type = CmdType.Rect, X = x, Y = y, W = w, H = h, C0 = color });
	}

	public void DrawGradientRectV(float x, float y, float w, float h, Color top, Color bot)
	{
		_cmds.Add(new Cmd { Type = CmdType.GradientV, X = x, Y = y, W = w, H = h, C0 = top, C1 = bot });
	}

	public void DrawGradientRectH(float x, float y, float w, float h, Color left, Color right)
	{
		_cmds.Add(new Cmd { Type = CmdType.GradientH, X = x, Y = y, W = w, H = h, C0 = left, C1 = right });
	}

	public void ClipRect(float x, float y, float w, float h)
	{
		_cmds.Add(new Cmd { Type = CmdType.Clip, X = x, Y = y, W = w, H = h });
	}

	public void ClearClip()
	{
		_cmds.Add(new Cmd { Type = CmdType.Unclip });
	}
	
	public float DrawString(string text, float x, float y, int color = 0xFFFFFF)
	{
		if (string.IsNullOrEmpty(text)) return x;
		if (!GlyphClipping.IsInitialized) GlyphClipping.Initialize();

		var col = HexToColor(color);
		var shadowCol = new Color(0.15f, 0.15f, 0.15f, col.A);
		bool bold = false, italic = false, obfuscated = false;
		var startX = x;

		for (var i = 0; i < text.Length;)
		{
			var c = text[i];
			if (TryParseFmt(text, i, out var skip, ref col, ref bold, ref italic, ref obfuscated))
			{
				i += skip;
				continue;
			}

			if (c == '\n') { x = startX; y += 16f; i++; continue; }

			int cp = c;
			if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
				cp = char.ConvertToUtf32(c, text[i += 2]);
			else i++;

			if (cp > 0xFFFF) continue;

			var glyphW = GlyphClipping.GetCharWidth(cp);
			if (glyphW <= 0f) glyphW = cp < 128 ? 8f : 16f;
			if (glyphW < 2f && cp == 32) glyphW = 4f;

			var page = cp < 128 ? -1 : cp >> 8;

			var asciiClipped = new GlyphClipping.ClippedGlyph();
			var unicodeClipped = new GlyphClipping.ClippedGlyph();
			var hasClipping = page == -1 ? 
				GlyphClipping.GetAsciiClipping(cp, out asciiClipped) && asciiClipped.HasContent :
				GlyphClipping.GetUnicodeClipping(cp, out unicodeClipped) && unicodeClipped.HasContent;
			
			if (hasClipping)
			{
				var clipped = page == -1 ? asciiClipped : unicodeClipped;
				var tex = page == -1 ? FontTexture.GetAsciiTexture() : FontTexture.GetPageTexture(page);
				if (tex != null)
				{
					_cmds.Add(MakeTexCmd(x + 1, y + 1, clipped, shadowCol, tex));
				}
			}
			
			if (bold)
			{
				if (hasClipping)
				{
					var clipped = page == -1 ? asciiClipped : unicodeClipped;
					var tex = page == -1 ? FontTexture.GetAsciiTexture() : FontTexture.GetPageTexture(page);
					if (tex != null)
					{
						_cmds.Add(MakeTexCmd(x + 1, y, clipped, col, tex));
					}
				}
			}
			
			if (hasClipping)
			{
				var clipped = page == -1 ? asciiClipped : unicodeClipped;
				var tex = page == -1 ? FontTexture.GetAsciiTexture() : FontTexture.GetPageTexture(page);
				if (tex != null)
				{
					_cmds.Add(MakeTexCmd(x, y, clipped, col, tex));
				}
			}

			advance: x += glyphW + (bold ? 1f : 0f);
		}

		return x;
	}

	public float GetStringWidth(string text)
	{
		if (string.IsNullOrEmpty(text)) return 0f;
		float maxW = 0f, curW = 0f;
		for (var i = 0; i < text.Length;)
		{
			var c = text[i];
			if (c == '\u00A7' && i + 1 < text.Length) { i += 2; continue; }
			if (c == '&' && i + 1 < text.Length && "0123456789abcdefABCDEFlmnokrLMNOKR".Contains(text[i + 1]))
			{
				i += 2; continue;
			}
			if (c == '\n') { if (curW > maxW) maxW = curW; curW = 0f; i++; continue; }
			int cp = c;
			if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
				cp = char.ConvertToUtf32(c, text[i += 2]);
			else i++;
			var w = GlyphClipping.GetCharWidth(cp);
			if (w <= 0f) w = cp < 128 ? 8f : 16f;
			curW += w;
		}
		return Math.Max(maxW, curW);
	}
	

	public static Color McColor(int idx) => McColors[idx & 0xF];

	public static Color HexToColor(int hex)
	{
		if ((hex & 0xFF000000) == 0) hex |= unchecked((int)0xFF000000);
		return new Color(
			((hex >> 16) & 0xFF) / 255f,
			((hex >> 8) & 0xFF) / 255f,
			(hex & 0xFF) / 255f,
			((hex >> 24) & 0xFF) / 255f
		);
	}

	public override void _Draw()
	{
		if (!_dirty || _cmds.Count == 0) return;
		_dirty = false;

		var clips = new Stack<Rect2>(8);

		foreach (var cmd in _cmds)
		{
			switch (cmd.Type)
			{
				case CmdType.Clip:
					clips.Push(new Rect2(cmd.X, cmd.Y, cmd.W, cmd.H));
					break;

				case CmdType.Unclip:
					if (clips.Count > 0) clips.Pop();
					break;

				case CmdType.TexRect:
					if (cmd.Tex == null) break;
					{
						var dst = new Rect2(cmd.X, cmd.Y, cmd.W, cmd.H);
						if (!IsVisible(dst, clips)) break;
						var tw = cmd.Tex.GetWidth();
						var th = cmd.Tex.GetHeight();
						var src = new Rect2(
							cmd.U0 * tw, cmd.V0 * th,
							(cmd.U1 - cmd.U0) * tw, (cmd.V1 - cmd.V0) * th);
						DrawTextureRectRegion(cmd.Tex, dst, src, cmd.C0);
					}
					break;

				case CmdType.Rect:
					{
						var dst = new Rect2(cmd.X, cmd.Y, cmd.W, cmd.H);
						if (IsVisible(dst, clips))
							DrawRect(dst, cmd.C0);
					}
					break;

				case CmdType.GradientV:
					{
						if (!IsVisible(new Rect2(cmd.X, cmd.Y, cmd.W, cmd.H), clips)) break;
						var n = 4;
						for (var b = 0; b < n; b++)
						{
							var t = (float)b / (n - 1);
							var bh = cmd.H / n;
							DrawRect(new Rect2(cmd.X, cmd.Y + bh * b, cmd.W, bh),
								cmd.C0.Lerp(cmd.C1, t));
						}
					}
					break;

				case CmdType.GradientH:
					{
						if (!IsVisible(new Rect2(cmd.X, cmd.Y, cmd.W, cmd.H), clips)) break;
						var n = 4;
						for (var b = 0; b < n; b++)
						{
							var t = (float)b / (n - 1);
							var bw = cmd.W / n;
							DrawRect(new Rect2(cmd.X + bw * b, cmd.Y, bw, cmd.H),
								cmd.C0.Lerp(cmd.C1, t));
						}
					}
					break;
			}
		}
	}

	private static bool IsVisible(Rect2 rect, Stack<Rect2> clips)
	{
		if (rect.Size.X <= 0 || rect.Size.Y <= 0) return false;
		foreach (var c in clips)
		{
			rect = rect.Intersection(c);
			if (!rect.HasArea()) return false;
		}
		return true;
	}


	private static Cmd MakeTexCmd(float x, float y, GlyphClipping.ClippedGlyph g,
		Color color, Texture2D tex)
	{
		float actualW = (g.U1 - g.U0) * tex.GetWidth();
		float actualH = (g.V1 - g.V0) * tex.GetHeight();
		
		return new Cmd
		{
			Type = CmdType.TexRect, X = x, Y = y, W = actualW, H = actualH,
			U0 = g.U0, V0 = g.V0, U1 = g.U1, V1 = g.V1,
			C0 = color, Tex = tex
		};
	}

	private static bool TryParseFmt(string s, int i, out int skip, ref Color col, ref bool bold, ref bool italic, ref bool obfuscated)
	{
		skip = 0;
		if (i >= s.Length) return false;

		if (s[i] == '\u00A7' && i + 1 < s.Length)
		{
			ApplyFmtCode(s[i + 1], ref col, ref bold, ref italic, ref obfuscated);
			skip = 2;
			return true;
		}

		if (s[i] == '&' && i + 1 < s.Length)
		{
			var n = s[i + 1];
			if ((n >= '0' && n <= '9') || (n >= 'a' && n <= 'f') || "lmnokr".Contains(char.ToLower(n)))
			{
				ApplyFmtCode(n, ref col, ref bold, ref italic, ref obfuscated);
				skip = 2;
				return true;
			}
		}

		return false;
	}

	private static void ApplyFmtCode(char code, ref Color col, ref bool bold, ref bool italic, ref bool obfuscated)
	{
		if (code >= '0' && code <= '9')
		{
			col = McColors[code - '0'];
			bold = italic = obfuscated = false;
		}
		else if (code >= 'a' && code <= 'f')
		{
			col = McColors[code - 'a' + 10];
			bold = italic = obfuscated = false;
		}
		else
		{
			switch (char.ToLower(code))
			{
				case 'l': bold = true; break;
				case 'o': italic = true; break;
				case 'k': obfuscated = true; break;
				case 'r':
					col = Colors.White;
					bold = italic = obfuscated = false;
					break;
			}
		}
	}

	public override void _ExitTree()
	{
		_cmds.Clear();
	}
}