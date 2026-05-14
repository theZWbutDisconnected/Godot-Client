using System.Collections.Generic;
using Godot;

namespace TestClient.Source.World;

public class ChunkData
{
    public const int Width = 16;
    public const int Height = 256;
    public const int Depth = 16;
    public const int SectionCount = Height / 16; // 16

    public int ChunkX { get; }
    public int ChunkZ { get; }

    private readonly ChunkSection?[] _sections = new ChunkSection[SectionCount];

    private byte[] _biomeArray = new byte[256];

    public bool Dirty { get; set; }

    public bool HasSkyLight { get; set; } = true;

    public ChunkData(int chunkX, int chunkZ)
    {
        ChunkX = chunkX;
        ChunkZ = chunkZ;
    }

    public ChunkSection? GetSection(int sectionIndex)
    {
        if (sectionIndex < 0 || sectionIndex >= SectionCount) return null;
        return _sections[sectionIndex];
    }

    public ChunkSection GetOrCreateSection(int sectionIndex)
    {
        if (sectionIndex < 0 || sectionIndex >= SectionCount)
            throw new System.ArgumentOutOfRangeException(nameof(sectionIndex));

        return _sections[sectionIndex] ??= new ChunkSection();
    }

    public int GetBlockId(int worldX, int worldY, int worldZ)
    {
        if (worldY < 0 || worldY >= Height) return 0;
        int lx = WorldToLocal(worldX, ChunkX);
        int lz = WorldToLocal(worldZ, ChunkZ);
        int sectionY = worldY >> 4;
        int localY = worldY & 0xF;

        var section = _sections[sectionY];
        if (section == null) return 0;

        return section.GetBlockId(lx, localY, lz);
    }

    public int GetMetadata(int worldX, int worldY, int worldZ)
    {
        if (worldY < 0 || worldY >= Height) return 0;
        int lx = WorldToLocal(worldX, ChunkX);
        int lz = WorldToLocal(worldZ, ChunkZ);
        int sectionY = worldY >> 4;
        int localY = worldY & 0xF;

        var section = _sections[sectionY];
        if (section == null) return 0;

        return section.GetMetadata(lx, localY, lz);
    }

    public void SetBlock(int worldX, int worldY, int worldZ, int blockId, int metadata = 0)
    {
        if (worldY < 0 || worldY >= Height) return;
        int lx = WorldToLocal(worldX, ChunkX);
        int lz = WorldToLocal(worldZ, ChunkZ);
        int sectionY = worldY >> 4;
        int localY = worldY & 0xF;

        GetOrCreateSection(sectionY).SetBlock(lx, localY, lz, blockId, metadata);
        Dirty = true;
    }

    public bool HasBlock(int worldX, int worldY, int worldZ)
    {
        if (worldY < 0 || worldY >= Height) return false;
        int lx = WorldToLocal(worldX, ChunkX);
        int lz = WorldToLocal(worldZ, ChunkZ);
        int sectionY = worldY >> 4;
        int localY = worldY & 0xF;

        var section = _sections[sectionY];
        if (section == null) return false;

        return section.GetBlockRaw(lx, localY, lz) != 0;
    }

    public byte[] GetBiomeArray() => _biomeArray;

    public ChunkSection?[] GetSections() => _sections;

    public static int WorldToLocal(int worldCoord, int chunkCoord)
    {
        return worldCoord - chunkCoord * Width;
    }

    public static int LocalToWorld(int localCoord, int chunkCoord)
    {
        return localCoord + chunkCoord * Width;
    }

    public static int WorldToChunk(int worldCoord)
    {
        return worldCoord >> 4; // floor(worldCoord / 16)
    }
}
