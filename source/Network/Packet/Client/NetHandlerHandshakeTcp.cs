using TestClient.Source.Network.Packet.Client.Handshake;
using TestClient.Source.Network.Packet.NetHandler;

namespace TestClient.Source.Network.Packet.Client;

public class NetHandlerHandshakeTcp : INetHandlerHandshake
{
    public void ProcessHandshake(C00Handshake packetIn)
    {
        switch (packetIn.RequestedState)
        {
            case ConnectionState.Login:
                break;
            case ConnectionState.Status:
                break;
        }
    }

    public void Disconnected(string reason)
    {
    }
}