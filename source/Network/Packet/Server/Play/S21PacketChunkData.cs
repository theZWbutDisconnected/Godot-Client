using TestClient.Source.Network.Packet;
using TestClient.Source.World;

namespace TestClient.Source.Network.Packet.Server.Play;

/// <summary>
/// MC 1.8.9 S21PacketChunkData 解析。
/// 
/// 包格式：
///   int    chunkX
///   int    chunkZ
///   bool   groundUpContinuous (是否包含生物群系数据)
///   ushort sectionBitmask    (哪些 section 有数据，bit i = section i)
///   byte[] data              (所有 section 的方块+光照 + 可选生物群系)
/// 
/// data 内部布局（按有数据的 section 顺序排列）：
///   对每个有数据的 section：
///     4096 chars 的方块数据 → 8192 bytes (每个 char 拆成低字节、高字节)
///     2048 bytes 方块光照 (nibble array)
///     [如果有天空光] 2048 bytes 天空光照 (nibble array)
///   [如果 groundUpContinuous] 256 bytes 生物群系
/// </summary>
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
        int sectionBitmask = buf.ReadUnsignedShort();
        byte[] data = buf.ReadBytes(buf.ReadVarInt());

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

        int dataOffset = 0;
        int sectionCount = System.Numerics.BitOperations.PopCount((uint)sectionBitmask);
        bool hasSkyLight = groundUpContinuous;

        for (int i = 0; i < ChunkData.SectionCount; i++)
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
            for (int i = 0; i < 256; i++)
                biomeArray[i] = data[dataOffset + i];
        }

        chunk.Dirty = true;
        return chunk;
    }
}
