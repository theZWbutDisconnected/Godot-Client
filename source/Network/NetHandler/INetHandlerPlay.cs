using TestClient.Source.Network.Packet.Server.Play;

namespace TestClient.Source.Network.NetHandler;

public interface INetHandlerPlayClient : INetHandler
{
    void HandleKeepAlive(S00KeepAlive @in);
    void HandleJoinGame(S01JoinGame @in);
    void HandlePlayerPosLook(S08PlayerPosLook @in);
    void HandleEntityTeleport(S18EntityTeleport @in);
    void HandleChunkData(S21ChunkData @in);
    void HandleMapChunkBulk(S26MapChunkBulk @in);
    void HandleConfirmTransaction(S32ConfirmTransaction @in);
    void HandleDisconnect(S40Disconnect @in);
}