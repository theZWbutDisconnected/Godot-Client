using TestClient.Source.Physics;

namespace TestClient.Source.Network.Packet.Server.Play;

public class S23BlockChange : IPacket
{
    public BlockPos BlockPos { get; private set; }
    public int BlockStateId { get; private set; }

    public void Read(PacketBuffer buf)
    {
        BlockPos = buf.ReadBlockPos();
        BlockStateId = buf.ReadVarInt();
    }

    public void Write(PacketBuffer buf)
    {
        // S2C only, no need to write
    }
}