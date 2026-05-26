using System;
using System.IO;

namespace TestClient.Source.Network.Packet.Client.Play;

public class ClientboundCustomPayload : IPacket
{
    public ClientboundCustomPayload()
    {
        Channel = "";
        Data = Array.Empty<byte>();
    }

    public ClientboundCustomPayload(string channel, byte[] data)
    {
        Channel = channel;
        Data = data;
    }

    public ClientboundCustomPayload(string channel, PacketBuffer dataBuffer)
    {
        Channel = channel;
        Data = ((MemoryStream)dataBuffer.GetInternalStream()).ToArray();
    }

    public string Channel { get; }
    public byte[] Data { get; }

    public void Read(PacketBuffer buf)
    {
        /* C2S only, no need to read */
    }

    public void Write(PacketBuffer buf)
    {
        buf.WriteString(Channel);
        buf.WriteRawBytes(Data);
    }
}