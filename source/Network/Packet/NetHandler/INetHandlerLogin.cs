using TestClient.Source.Network.Packet.Server.Login;

namespace TestClient.Source.Network.Packet.NetHandler;

public interface INetHandlerLoginClient : INetHandler
{
    void HandleEncryptionRequest(S01PacketEncryptionRequest packetIn);
    void HandleLoginSuccess(S02PacketLoginSuccess packetIn);
    void HandleDisconnect(S00Disconnect packetIn);
    void HandleEnableCompression(S03PacketEnableCompression packetIn);
}