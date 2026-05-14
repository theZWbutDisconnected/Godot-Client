using System;
using System.Collections.Generic;
using System.Numerics;
using TestClient.Source.World;

namespace TestClient.Source.Network.Packet.Server.Play;

public class S26PacketMapChunkBulk : IPacket
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
            var sectionCount = BitOperations.PopCount((uint)dataSizes[i]);
            var dataSize = CalculateDataSize(dataSizes[i], IsOverworld, true);
            var data = buf.ReadBytes(dataSize);
            
            var chunk = S21PacketChunkData.ParseChunkData(xPositions[i], zPositions[i], 
                dataSizes[i], true, data);
            Chunks.Add(chunk);
        }
        
        Console.WriteLine($"Dispatching 0x26: {chunkCount} chunks");
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
        
        // Lit: 2048 bytes per section
        size += sectionCount * 2048;
        
        // Sky lit: 2048 bytes per section (OverWorld)
        if (isOverworld)
            size += sectionCount * 2048;
        
        // Biome data: 256 bytes (only groundUpContinuous)
        if (groundUpContinuous)
            size += 256;
        
        return size;
    }
}
