namespace TestClient.Source.Network.Packet.Server.Play;

public class S32ConfirmTransaction : IPacket
{
    public int WindowId { get; private set; }
    public int ActionNumber { get; private set; }
    public bool Accepted { get; private set; }

    public void Read(PacketBuffer buf)
    {
        WindowId = buf.ReadByte();
        ActionNumber = buf.ReadShort();
        Accepted = buf.ReadBoolean();
    }

    public void Write(PacketBuffer buf)
    {
        /* S2C only, no need to write */
    }
}