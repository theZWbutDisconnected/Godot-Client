using System;
using System.Numerics;
using Godot;
using TestClient.Source.World;

namespace TestClient.Source.Network.Packet.Server.Play;

public class ServerboundChunkData : IPacket
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
        var dataLength = buf.ReadVarInt();
        var data = buf.ReadByteArray(dataLength);

        Chunk = ParseChunkData(ChunkX, ChunkZ, sectionBitmask, GroundUpContinuous, data);

        try
        {
            var blockEntityCount = buf.ReadVarInt();
            for (var i = 0; i < blockEntityCount; i++) SkipNbtTag(buf);
        }
        catch (Exception)
        {
        }
    }

    public void Write(PacketBuffer buf)
    {
        /* S2C only */
    }

    private static void SkipNbtTag(PacketBuffer buf)
    {
        try
        {
            var tagType = buf.ReadByte();
            if (tagType == 0) return; // TAG_End

            var nameLength = buf.ReadUnsignedShort();
            if (nameLength > 0)
                buf.ReadByteArray(nameLength);

            SkipNbtPayload(buf, tagType);
        }
        catch (Exception e)
        {
            GD.PushError("Error parsing NBT data: " + e.Message);
        }
    }

    private static void SkipNbtPayload(PacketBuffer buf, sbyte tagType)
    {
        switch (tagType)
        {
            case 1: // TAG_Byte
                buf.ReadByte();
                break;
            case 2: // TAG_Short
                buf.ReadShort();
                break;
            case 3: // TAG_Int
                buf.ReadInt();
                break;
            case 4: // TAG_Long
                buf.ReadLong();
                break;
            case 5: // TAG_Float
                buf.ReadFloat();
                break;
            case 6: // TAG_Double
                buf.ReadDouble();
                break;
            case 7: // TAG_Byte_Array
                var byteArrayLength = buf.ReadInt();
                if (byteArrayLength > 0)
                    buf.ReadByteArray(byteArrayLength);
                break;
            case 8: // TAG_String
                var stringLength = buf.ReadUnsignedShort();
                if (stringLength > 0)
                    buf.ReadByteArray(stringLength);
                break;
            case 9: // TAG_List
                var listType = buf.ReadByte();
                var listLength = buf.ReadInt();
                for (var i = 0; i < listLength; i++)
                    SkipNbtPayload(buf, listType);
                break;
            case 10: // TAG_Compound
                while (true)
                {
                    var childType = buf.ReadByte();
                    if (childType == 0) break; // TAG_End
                    var childNameLength = buf.ReadUnsignedShort();
                    if (childNameLength > 0)
                        buf.ReadByteArray(childNameLength);
                    SkipNbtPayload(buf, childType);
                }

                break;
            case 11: // TAG_Int_Array
                var intArrayLength = buf.ReadInt();
                if (intArrayLength > 0)
                    buf.ReadByteArray(intArrayLength * 4);
                break;
            default:
                throw new InvalidOperationException($"Unknown NBT tag type: {tagType}");
        }
    }

    public static ChunkData ParseChunkData(int chunkX, int chunkZ, int sectionBitmask,
        bool groundUpContinuous, byte[] data)
    {
        var chunk = new ChunkData(chunkX, chunkZ);
        chunk.HasSkyLight = true;

        var dataOffset = 0;
        var sectionCount = BitOperations.PopCount((uint)sectionBitmask);

        // Block data
        for (var i = 0; i < ChunkData.SectionCount; i++)
        {
            if ((sectionBitmask & (1 << i)) == 0) continue;
            var section = chunk.GetOrCreateSection(i);
            section.ReadBlocksFromPacket(data, dataOffset);
            dataOffset += ChunkSection.TotalBlocks * 2; // 8192
        }

        // Lit data
        for (var i = 0; i < ChunkData.SectionCount; i++)
        {
            if ((sectionBitmask & (1 << i)) == 0) continue;
            var section = chunk.GetSection(i);
            if (section == null) continue;
            ChunkSection.ReadNibbleArray(data, section.GetBlockLightArray(), dataOffset,
                ChunkSection.TotalBlocks / 2);
            dataOffset += ChunkSection.TotalBlocks / 2; // 2048
        }

        // Sky light (OverWorld)
        for (var i = 0; i < ChunkData.SectionCount; i++)
        {
            if ((sectionBitmask & (1 << i)) == 0) continue;
            var section = chunk.GetSection(i);
            if (section == null) continue;
            ChunkSection.ReadNibbleArray(data, section.GetSkyLightArray(), dataOffset,
                ChunkSection.TotalBlocks / 2);
            dataOffset += ChunkSection.TotalBlocks / 2; // 2048
        }

        // Biome data (only groundUpContinuous)
        if (groundUpContinuous)
        {
            var biomeArray = chunk.GetBiomeArray();
            for (var i = 0; i < 256; i++)
                biomeArray[i] = data[dataOffset + i];
            dataOffset += 256;
        }

        chunk.Dirty = true;
        return chunk;
    }
}