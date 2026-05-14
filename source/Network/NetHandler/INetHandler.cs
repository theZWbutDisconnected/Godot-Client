namespace TestClient.Source.Network.NetHandler;

public interface INetHandler
{
    void Disconnected(string reason);
}