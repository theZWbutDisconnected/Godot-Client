using TestClient.Source.Network.Packet.Server.Login;

namespace TestClient.Source.Network.Packet.NetHandler;

public interface INetHandlerLoginClient : INetHandler
{
    void HandleEncryptionRequest(S01EncryptionRequest @in);
    void HandleLoginSuccess(S02LoginSuccess @in);
    void HandleDisconnect(S00Disconnect packetIn);
    void HandleEnableCompression(S03EnableCompression @in);
}