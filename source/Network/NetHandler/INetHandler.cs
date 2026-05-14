namespace TestClient.Source.Network.Packet.NetHandler;

public interface INetHandler
{
    void Disconnected(string reason);
}