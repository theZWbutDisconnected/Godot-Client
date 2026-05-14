namespace TestClient.Source.Network.Packet.Server.Play;

public class S40Disconnect : IPacket
{
    public string Reason { get; private set; }

    public S40Disconnect()
    {
    }

    public S40Disconnect(string reasonIn)
    {
        Reason = reasonIn;
    }

    public void Read(PacketBuffer buf)
    {
        Reason = buf.ReadChatComponent();
    }

    public void Write(PacketBuffer buf)
    {
        /* S2C only, no need to write */
    }
}