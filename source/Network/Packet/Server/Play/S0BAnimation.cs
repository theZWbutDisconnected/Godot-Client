namespace TestClient.Source.Network.Packet.Server.Play;

public class S0BAnimation : IPacket
{
    public int EntityId { get; private set; }
    public int Type { get; private set; }

    public void Read(PacketBuffer buf)
    {
        EntityId = buf.ReadVarInt();
        Type = buf.ReadUnsignedByte();
    }

    public void Write(PacketBuffer buf)
    {
        /* S2C only, no need to write */
    }
}
