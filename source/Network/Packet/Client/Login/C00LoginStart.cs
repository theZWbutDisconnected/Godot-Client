namespace TestClient.Source.Network.Packet.Client.Login;

public class C00LoginStart(string profileIn) : IPacket
{
    public string Name { get; } = profileIn;

    public void Read(PacketBuffer buf)
    {
        /* C2S only, no need to read */
    }

    public void Write(PacketBuffer buf)
    {
        buf.WriteString(Name);
    }
}