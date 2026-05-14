using TestClient.Source.Network.Packet.Server.Play;

namespace TestClient.Source.Network.NetHandler;

public interface INetHandlerPlayClient : INetHandler
{
    void HandleKeepAlive(S00KeepAlive packetIn);
    void HandleJoinGame(S01JoinGame packetIn);
    void HandleConfirmTransaction(S32ConfirmTransaction packetIn);
    void HandleDisconnect(S40Disconnect packetIn);
}