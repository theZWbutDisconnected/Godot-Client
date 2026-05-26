using System.Collections.Generic;
using System.Numerics;
using TestClient.Source.World;

namespace TestClient.Source.Network.Packet.Server.Play;

public class ServerboundMapChunkBulk : IPacket
{
    public bool IsOverworld { get; private set; }
    public List<ChunkData> Chunks { get; } = new();

    public void Read(PacketBuffer buf)
    {
        IsOverworld = buf.ReadBoolean();
        var chunkCount = buf.ReadVarInt();

        // Metadata
        var xPositions = new int[chunkCount];
        var zPositions = new int[chunkCount];
        var dataSizes = new int[chunkCount];

        for (var i = 0; i < chunkCount; i++)
        {
            xPositions[i] = buf.ReadInt();
            zPositions[i] = buf.ReadInt();
            dataSizes[i] = buf.ReadUnsignedShort();
        }

        // Chunk data
        for (var i = 0; i < chunkCount; i++)
        {
            var dataSize = CalculateDataSize(dataSizes[i], IsOverworld, true);
            var data = buf.ReadByteArray(dataSize);

            var chunk = ServerboundChunkData.ParseChunkData(xPositions[i], zPositions[i],
                dataSizes[i], true, data);
            Chunks.Add(chunk);
        }
    }

    public void Write(PacketBuffer buf)
    {
        /* S2C only */
    }

    private int CalculateDataSize(int sectionBitmask, bool isOverworld, bool groundUpContinuous)
    {
        var sectionCount = BitOperations.PopCount((uint)sectionBitmask);
        var size = 0;

        // Blocks: 4096 * 2 = 8192 bytes per section
        size += sectionCount * 8192;

        // Block light: 2048 bytes per section
        size += sectionCount * 2048;

        // Sky light: 2048 bytes per section (only OverWorld)
        if (isOverworld)
            size += sectionCount * 2048;

        // Biome data: 256 bytes (only GroundUpContinuous)
        if (groundUpContinuous)
            size += 256;

        return size;
    }
}