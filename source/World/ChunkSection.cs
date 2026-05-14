using Godot;

namespace TestClient.Source.World;

public class ChunkSection
{
    public const int Width = 16;
    public const int Height = 16;
    public const int Depth = 16;
    public const int TotalBlocks = Width * Height * Depth; // 4096

    private readonly char[] _blockData = new char[TotalBlocks];

    private readonly byte[] _blockLight = new byte[TotalBlocks / 2];

    private readonly byte[] _skyLight = new byte[TotalBlocks / 2];

    public bool IsEmpty { get; private set; } = true;

    public void SetBlock(int localX, int localY, int localZ, int blockId, int metadata = 0)
    {
        if (!IsInBounds(localX, localY, localZ)) return;
        _blockData[GetIndex(localX, localY, localZ)] = (char)((blockId & 0xFFF) | ((metadata & 0xF) << 12));
        IsEmpty = false;
    }

    public void SetBlockRaw(int index, char value)
    {
        _blockData[index] = value;
        if (value != 0) IsEmpty = false;
    }

    public int GetBlockId(int localX, int localY, int localZ)
    {
        return _blockData[GetIndex(localX, localY, localZ)] & 0xFFF;
    }

    public int GetMetadata(int localX, int localY, int localZ)
    {
        return (_blockData[GetIndex(localX, localY, localZ)] >> 12) & 0xF;
    }

    public char GetBlockRaw(int localX, int localY, int localZ)
    {
        return _blockData[GetIndex(localX, localY, localZ)];
    }

    public char[] GetData() => _blockData;

    public byte[] GetBlockLightArray() => _blockLight;

    public byte[] GetSkyLightArray() => _skyLight;

    public void RecalcEmpty()
    {
        IsEmpty = true;
        for (int i = 0; i < TotalBlocks; i++)
        {
            if (_blockData[i] != 0)
            {
                IsEmpty = false;
                return;
            }
        }
    }

    public void ReadBlocksFromPacket(byte[] packetData, int offset)
    {
        for (int i = 0; i < TotalBlocks; i++)
        {
            int bi = offset + i * 2;
            _blockData[i] = (char)((packetData[bi] & 0xFF) | ((packetData[bi + 1] & 0xFF) << 8));
        }
        RecalcEmpty();
    }

    public static void ReadNibbleArray(byte[] src, byte[] dst, int srcOffset, int length)
    {
        for (int i = 0; i < length; i++)
        {
            byte val = src[srcOffset + i];
            dst[i] = val;
        }
    }

    private static int GetIndex(int x, int y, int z) => y * Width * Depth + z * Width + x;

    private static bool IsInBounds(int x, int y, int z) =>
        x >= 0 && x < Width && y >= 0 && y < Height && z >= 0 && z < Depth;
}
