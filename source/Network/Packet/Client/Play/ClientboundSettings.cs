namespace TestClient.Source.Network.Packet.Client.Play;

public class ClientboundSettings : IPacket
{
    public string Language { get; set; } = "en_US";
    public byte ViewDistance { get; set; } = 10;
    public byte ChatMode { get; set; } = 0;
    public bool ChatColors { get; set; } = true;
    public byte SkinParts { get; set; } = 0xFF;

    public void Read(PacketBuffer buf)
    {
        /* C2S only */
    }

    public void Write(PacketBuffer buf)
    {
        buf.WriteString(Language);
        buf.WriteByte(ViewDistance);
        buf.WriteByte(ChatMode);
        buf.WriteBoolean(ChatColors);
        buf.WriteByte(SkinParts);
    }
}