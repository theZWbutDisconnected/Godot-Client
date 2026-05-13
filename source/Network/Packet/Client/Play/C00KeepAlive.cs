namespace TestClient.Source.Network.Packet.Client.Play;

public class C00KeepAlive(int keepAliveId) : IPacket
{
    public int KeepAliveId { get; } = keepAliveId;

    public void Read(PacketBuffer buf)
    {
        /* C2S only, no need to read */
    }

    public void Write(PacketBuffer buf)
    {
        buf.WriteVarInt(KeepAliveId);
    }
}