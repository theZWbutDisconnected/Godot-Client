using TestClient.Source.Physics;
using TestClient.Source.World.Tile;

namespace TestClient.Source.Network.Packet.Server.Play;

public class S24BlockAction : IPacket
{
    public BlockPos BlockPosition { get; private set; }
    public int Instrument { get; private set; }
    public int Pitch { get; private set; }
    public Block Block { get; private set; }

    public void Read(PacketBuffer buf)
    {
        BlockPosition = buf.ReadBlockPos();
        Instrument = buf.ReadUnsignedByte();
        Pitch = buf.ReadUnsignedByte();
        Block = Blocks.GetPreset(buf.ReadVarInt() & 4095);
    }

    public void Write(PacketBuffer buf)
    {
        // S2C only, no need to write
    }
}