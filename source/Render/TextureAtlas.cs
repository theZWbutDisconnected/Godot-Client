using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;

namespace TestClient.Source.Render;

public static class TextureAtlas
{
	public const int TileSize = 16;
	public static readonly int AtlasWidth;
	public static readonly float TileUVSize;
	public static readonly ImageTexture AtlasTexture;

	private static readonly Dictionary<string, int> NameToIndex;
	private static readonly Dictionary<int, AnimData> Animations = new();
	private static readonly HashSet<int> AnimatedTexIds = new();

	public struct AnimData
	{
		public int FrameCount;
		public float FrameTime;
		public bool PingPong;
		public int FrameOffset;
	}

	static TextureAtlas()
	{
		var pngPaths = Directory.GetFiles("assets/blocks/", "*.png")
			.Select(p => p.Replace('\\', '/'))
			.OrderBy(p => p, StringComparer.Ordinal)
			.ToList();

		var staticNames = new List<string>();
		var animEntries = new List<AnimEntry>();

		foreach (var path in pngPaths)
		{
			var name = Path.GetFileNameWithoutExtension(path);
			var mcmetaPath = path + ".mcmeta";

			if (File.Exists(mcmetaPath))
			{
				var anim = ParseMcmeta(mcmetaPath, path, out int frameCount, out int tileFrames);
				animEntries.Add(new AnimEntry
				{
					Name = name,
					Path = path,
					TileFrames = tileFrames,
					Data = anim
				});
			}
			else
			{
				staticNames.Add(name);
			}
		}

		int totalTiles = staticNames.Count + animEntries.Sum(e => e.TileFrames);
		AtlasWidth = (int)Math.Ceiling(Math.Sqrt(totalTiles));
		TileUVSize = 1.0f / AtlasWidth;

		int atlasPixelSize = AtlasWidth * TileSize;
		var atlasImage = Image.Create(atlasPixelSize, atlasPixelSize, false, Image.Format.Rgba8);

		NameToIndex = new Dictionary<string, int>(totalTiles);
		int tileIndex = 0;

		foreach (var name in staticNames)
		{
			PlaceTile(atlasImage, $"assets/blocks/{name}.png", tileIndex, TileSize, TileSize);
			NameToIndex[name] = tileIndex;
			tileIndex++;
		}

		foreach (var entry in animEntries)
		{
			int firstFrameIndex = tileIndex;
			NameToIndex[entry.Name] = firstFrameIndex;

			var img = new Image();
			if (img.Load(entry.Path) != Error.Ok)
			{
				GD.PushWarning($"[TextureAtlas] Failed to load animated: {entry.Path}");
				img.Resize(TileSize, TileSize, Image.Interpolation.Nearest);
				PlaceTileImage(atlasImage, img, tileIndex);
				tileIndex++;
				continue;
			}

			int frameHeight = img.GetWidth() > TileSize ? TileSize * 2 : TileSize;
			int framesInPng = img.GetHeight() / frameHeight;

			if (img.GetWidth() > TileSize)
			{
				img.Resize(TileSize, framesInPng * TileSize, Image.Interpolation.Nearest);
				frameHeight = TileSize;
			}

			for (int f = 0; f < entry.TileFrames; f++)
			{
				var frameImg = img.GetRegion(new Rect2I(0, f * TileSize, TileSize, TileSize));
				PlaceTileImage(atlasImage, frameImg, tileIndex);
				tileIndex++;
			}

			Animations[firstFrameIndex] = entry.Data;
			AnimatedTexIds.Add(firstFrameIndex);
		}

		AtlasTexture = ImageTexture.CreateFromImage(atlasImage);
	}

	public static int Index(string name)
	{
		return NameToIndex.TryGetValue(name, out var idx) ? idx : 0;
	}

	public static bool IsAnimated(int texIndex)
	{
		return AnimatedTexIds.Contains(texIndex);
	}

	public static AnimData? GetAnimData(int texIndex)
	{
		return Animations.TryGetValue(texIndex, out var d) ? d : null;
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

	private static AnimData ParseMcmeta(string mcmetaPath, string pngPath, out int frameCount, out int tileFrames)
	{
		string jsonText = File.ReadAllText(mcmetaPath);
		using var doc = JsonDocument.Parse(jsonText);
		var root = doc.RootElement;

		if (!root.TryGetProperty("animation", out var animElem))
		{
			frameCount = 1;
			tileFrames = 1;
			return new AnimData { FrameCount = 1, FrameTime = 1f };
		}

		float frameTime = 0.05f;
		if (animElem.TryGetProperty("frametime", out var ftElem))
			frameTime = ftElem.GetInt32() * 0.05f;

		var img = new Image();
		if (img.Load(pngPath) != Error.Ok)
		{
			frameCount = 1;
			tileFrames = 1;
			return new AnimData { FrameCount = 1, FrameTime = frameTime };
		}

		int frameHeight = img.GetWidth() > TileSize ? TileSize * 2 : TileSize;
		int pngFrames = img.GetHeight() / frameHeight;
		img.Dispose();

		bool pingPong = false;
		int frameOffset = 0;
		int[] frameIndices = null;

		if (animElem.TryGetProperty("frames", out var framesElem))
		{
			var indices = new List<int>();
			foreach (var f in framesElem.EnumerateArray())
			{
				if (f.ValueKind == JsonValueKind.Number)
				{
					indices.Add(f.GetInt32());
				}
			}

			if (indices.Count > 0)
			{
				frameIndices = indices.ToArray();
				int maxIdx = indices.Max();

				int n = indices.Count;
				if (n > 2)
				{
					int mid = (n + 1) / 2;
					bool isPingPong = true;
					for (int i = 0; i < mid && isPingPong; i++)
					{
						if (indices[i] != i) isPingPong = false;
					}
					for (int i = mid; i < n && isPingPong; i++)
					{
						if (indices[i] != n - i) isPingPong = false;
					}
					if (isPingPong)
					{
						pingPong = true;
						frameCount = n;
						tileFrames = pngFrames;
						return new AnimData
						{
							FrameCount = frameCount,
							FrameTime = frameTime,
							PingPong = true,
							FrameOffset = 0
						};
					}
				}

				if (!pingPong && indices[0] > 0)
				{
					frameOffset = indices[0];
				}

				frameCount = n;
				tileFrames = maxIdx + 1;
				return new AnimData
				{
					FrameCount = frameCount,
					FrameTime = frameTime,
					PingPong = false,
					FrameOffset = frameOffset
				};
			}
		}

		frameCount = pngFrames;
		tileFrames = pngFrames;
		return new AnimData
		{
			FrameCount = frameCount,
			FrameTime = frameTime,
			PingPong = false,
			FrameOffset = 0
		};
	}

	private static void PlaceTile(Image atlas, string path, int tileIndex, int w, int h)
	{
		var img = new Image();
		if (img.Load(path) != Error.Ok)
		{
			GD.PushWarning($"[TextureAtlas] Failed to load: {path}");
			return;
		}

		img.Resize(w, h, Image.Interpolation.Nearest);
		PlaceTileImage(atlas, img, tileIndex);
	}

	private static void PlaceTileImage(Image atlas, Image img, int tileIndex)
	{
		if (img.GetFormat() != atlas.GetFormat())
		{
			img.Convert(atlas.GetFormat());
		}
		
		int col = tileIndex % AtlasWidth;
		int row = tileIndex / AtlasWidth;
		atlas.BlitRect(img,
			new Rect2I(0, 0, TileSize, TileSize),
			new Vector2I(col * TileSize, row * TileSize));
	}

	private class AnimEntry
	{
		public string Name;
		public string Path;
		public int TileFrames;
		public AnimData Data;
	}
}
