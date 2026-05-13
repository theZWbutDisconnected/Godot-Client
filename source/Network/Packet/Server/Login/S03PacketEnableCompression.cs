namespace TestClient.Source.Network.Packet.Server.Login;

public class S03PacketEnableCompression : IPacket
{
    public int Threshold { get; private set; }

    public void Read(PacketBuffer buf)
    {
        Threshold = buf.ReadVarInt();
    }

    public void Write(PacketBuffer buf)
    {
        /* S2C only, no need to write */
    }
}