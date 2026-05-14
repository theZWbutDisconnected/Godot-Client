using TestClient.Source.Network.Packet.Client.Handshake;

namespace TestClient.Source.Network.Packet.NetHandler;

public interface INetHandlerHandshake : INetHandler
{
    void ProcessHandshake(C00Handshake packetIn);
}