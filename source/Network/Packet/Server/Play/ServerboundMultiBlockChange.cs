using TestClient.Source.Physics;
using TestClient.Source.World;

namespace TestClient.Source.Network.Packet.Server.Play;

public class ServerboundMultiBlockChange : IPacket
{
    public ChunkCoordIntPair ChunkPos { get; private set; }
    public BlockUpdateData[] ChangedBlocks { get; private set; }

    public void Read(PacketBuffer buf)
    {
        var chunkX = buf.ReadInt();
        var chunkZ = buf.ReadInt();
        ChunkPos = new ChunkCoordIntPair(chunkX, chunkZ);

        var count = buf.ReadVarInt();
        ChangedBlocks = new BlockUpdateData[count];

        for (var i = 0; i < count; ++i)
        {
            var crammedPos = buf.ReadShort();
            var blockStateId = buf.ReadVarInt();
            ChangedBlocks[i] = new BlockUpdateData(crammedPos, blockStateId);
        }
    }

    public void Write(PacketBuffer buf)
    {
        // S2C only, no need to write
    }

    public class BlockUpdateData
    {
        private readonly short _chunkPosCrammed;

        public BlockUpdateData(short crammedPos, int blockStateId)
        {
            _chunkPosCrammed = crammedPos;
            BlockStateId = blockStateId;
        }

        public int BlockStateId { get; }

        public BlockPos GetPos(ChunkCoordIntPair chunkCoord)
        {
            var x = chunkCoord.ChunkXPos * 16 + ((_chunkPosCrammed >> 12) & 15);
            var y = _chunkPosCrammed & 255;
            var z = chunkCoord.ChunkZPos * 16 + ((_chunkPosCrammed >> 8) & 15);
            return new BlockPos(x, y, z);
        }
    }
}