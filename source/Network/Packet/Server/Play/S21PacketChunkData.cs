using System.Numerics;
using TestClient.Source.World;

namespace TestClient.Source.Network.Packet.Server.Play;

public class S21PacketChunkData : IPacket
{
    public int ChunkX { get; private set; }
    public int ChunkZ { get; private set; }
    public bool GroundUpContinuous { get; private set; }
    public ChunkData? Chunk { get; private set; }

    public void Read(PacketBuffer buf)
    {
        ChunkX = buf.ReadInt();
        ChunkZ = buf.ReadInt();
        GroundUpContinuous = buf.ReadBoolean();
        var sectionBitmask = buf.ReadUnsignedShort();
        var data = buf.ReadBytes(buf.ReadVarInt());

        Chunk = ParseChunkData(ChunkX, ChunkZ, sectionBitmask, GroundUpContinuous, data);
    }

    public void Write(PacketBuffer buf)
    {
        /* S2C only */
    }

    public static ChunkData ParseChunkData(int chunkX, int chunkZ, int sectionBitmask,
        bool groundUpContinuous, byte[] data)
    {
        var chunk = new ChunkData(chunkX, chunkZ);
        chunk.HasSkyLight = true;

        var dataOffset = 0;
        var sectionCount = BitOperations.PopCount((uint)sectionBitmask);
        var hasSkyLight = groundUpContinuous;

        for (var i = 0; i < ChunkData.SectionCount; i++)
        {
            if ((sectionBitmask & (1 << i)) == 0) continue;

            var section = chunk.GetOrCreateSection(i);

            // Data: 4096 chars = 8192 bytes
            section.ReadBlocksFromPacket(data, dataOffset);
            dataOffset += ChunkSection.TotalBlocks * 2; // 8192

            // Lit: 2048 bytes
            ChunkSection.ReadNibbleArray(data, section.GetBlockLightArray(), dataOffset,
                ChunkSection.TotalBlocks / 2);
            dataOffset += ChunkSection.TotalBlocks / 2; // 2048

            // SkyLit: 2048 bytes (OverWorld & GroundUpContinuous)
            if (hasSkyLight)
            {
                ChunkSection.ReadNibbleArray(data, section.GetSkyLightArray(), dataOffset,
                    ChunkSection.TotalBlocks / 2);
                dataOffset += ChunkSection.TotalBlocks / 2; // 2048
            }
        }

        // Biome data：256 bytes（only groundUpContinuous）
        if (groundUpContinuous)
        {
            var biomeArray = chunk.GetBiomeArray();
            for (var i = 0; i < 256; i++)
                biomeArray[i] = data[dataOffset + i];
        }

        chunk.Dirty = true;
        return chunk;
    }
}