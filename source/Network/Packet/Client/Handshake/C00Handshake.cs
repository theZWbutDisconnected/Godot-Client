namespace TestClient.Source.Network.Packet.Client.Handshake;

public class C00Handshake(int version, string ip, int port, ConnectionState requestedState) : IPacket
{
    public int ProtocolVersion { get; } = version;
    public string Ip { get; } = ip;
    public int Port { get; } = port;
    public ConnectionState RequestedState { get; } = requestedState;

    public void Read(PacketBuffer buf)
    {
        /* C2S only, no need to read */
    }

    public void Write(PacketBuffer buf)
    {
        buf.WriteVarInt(ProtocolVersion);
        buf.WriteString(Ip);
        buf.WriteUnsignedShort(Port);
        buf.WriteVarInt((int)RequestedState);
    }
}