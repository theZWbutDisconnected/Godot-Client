using TestClient.Source.Network;

namespace TestClient.Source.Network.Packet.Server.Play;

public class ServerboundEntityAttach : IPacket
{
    public int Leash { get; private set; }
    public int EntityId { get; private set; }
    public int VehicleEntityId { get; private set; }

    public ServerboundEntityAttach()
    {
    }

    public void Read(PacketBuffer buf)
    {
        EntityId = buf.ReadInt();
        VehicleEntityId = buf.ReadInt();
        Leash = buf.ReadUnsignedByte();
    }

    public void Write(PacketBuffer stream)
    {
        /* S2C only */
    }
}
