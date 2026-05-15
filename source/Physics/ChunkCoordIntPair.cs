using TestClient.Source.Physics;

namespace TestClient.Source.World;

public class ChunkCoordIntPair
{
    public int ChunkXPos { get; }
    public int ChunkZPos { get; }
    private int _cachedHashCode;

    public ChunkCoordIntPair(int x, int z)
    {
        ChunkXPos = x;
        ChunkZPos = z;
    }

    public static long ChunkXZ2Int(int x, int z)
    {
        return ((long)x & 4294967295L) | (((long)z & 4294967295L) << 32);
    }

    public override int GetHashCode()
    {
        if (_cachedHashCode == 0)
        {
            int i = 1664525 * ChunkXPos + 1013904223;
            int j = 1664525 * (ChunkZPos ^ -559038737) + 1013904223;
            _cachedHashCode = i ^ j;
        }
        return _cachedHashCode;
    }

    public override bool Equals(object obj)
    {
        if (this == obj) return true;
        if (!(obj is ChunkCoordIntPair other)) return false;
        return ChunkXPos == other.ChunkXPos && ChunkZPos == other.ChunkZPos;
    }

    public int GetCenterXPos()
    {
        return (ChunkXPos << 4) + 8;
    }

    public int GetCenterZPosition()
    {
        return (ChunkZPos << 4) + 8;
    }

    public int GetXStart()
    {
        return ChunkXPos << 4;
    }

    public int GetZStart()
    {
        return ChunkZPos << 4;
    }

    public int GetXEnd()
    {
        return (ChunkXPos << 4) + 15;
    }

    public int GetZEnd()
    {
        return (ChunkZPos << 4) + 15;
    }

    public BlockPos GetBlock(int x, int y, int z)
    {
        return new BlockPos((ChunkXPos << 4) + x, y, (ChunkZPos << 4) + z);
    }

    public BlockPos GetCenterBlock(int y)
    {
        return new BlockPos(GetCenterXPos(), y, GetCenterZPosition());
    }

    public override string ToString()
    {
        return $"[{ChunkXPos}, {ChunkZPos}]";
    }
}