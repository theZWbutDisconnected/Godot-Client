namespace TestClient.Source.Network.Packet.Client.Play;

public class ClientboundConfirmTransaction : IPacket
{
    public ClientboundConfirmTransaction()
    {
    }

    public ClientboundConfirmTransaction(int windowId, int uid, bool accepted)
    {
        WindowId = windowId;
        Uid = uid;
        Accepted = accepted;
    }

    public int WindowId { get; }
    public int Uid { get; }
    public bool Accepted { get; }

    public void Read(PacketBuffer buf)
    {
        /* C2S only, no need to read */
    }

    public void Write(PacketBuffer buf)
    {
        buf.WriteByte(WindowId);
        buf.WriteShort(Uid);
        buf.WriteByte(Accepted ? 1 : 0);
    }
}