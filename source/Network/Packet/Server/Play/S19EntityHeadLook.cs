namespace TestClient.Source.Network.Packet.Server.Play;

public class S19EntityHeadLook : IPacket
{
    public int EntityId { get; private set; }
    public byte Yaw { get; private set; }

    public void Read(PacketBuffer buf)
    {
        EntityId = buf.ReadVarInt();
        Yaw = buf.ReadByte();
    }

    public void Write(PacketBuffer stream)
    {
        /* S2C only */
    }
}