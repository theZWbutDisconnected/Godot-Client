using System;
using System.IO;

namespace TestClient.Source.Network.Packet.Client.Play;

public class C17CustomPayload : IPacket
{
    public string Channel { get; }
    public byte[] Data { get; }

    public C17CustomPayload()
    {
        Channel = "";
        Data = Array.Empty<byte>();
    }

    public C17CustomPayload(string channel, byte[] data)
    {
        Channel = channel;
        Data = data;
    }

    public C17CustomPayload(string channel, PacketBuffer dataBuffer)
    {
        Channel = channel;
        Data = ((MemoryStream)dataBuffer.GetInternalStream()).ToArray();
    }

    public void Read(PacketBuffer buf) { /* C2S only, no need to read */ }

    public void Write(PacketBuffer buf)
    {
        buf.WriteString(Channel);
        buf.WriteRawBytes(Data);
    }
}
