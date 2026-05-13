using System;
using System.IO;

namespace TestClient.Source.Network.Packet.Client.Play;

public class C17PacketCustomPayload : IPacket
{
    public string Channel { get; }
    public byte[] Data { get; }

    public C17PacketCustomPayload()
    {
        Channel = "";
        Data = Array.Empty<byte>();
    }

    public C17PacketCustomPayload(string channel, byte[] data)
    {
        Channel = channel;
        Data = data;
    }

    public C17PacketCustomPayload(string channel, PacketBuffer dataBuffer)
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
