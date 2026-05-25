using TestClient.Source.Network.Packet.Server.Play;

namespace TestClient.Source.Network.NetHandler;

public interface INetHandlerPlayClient : INetHandler
{
    void HandleKeepAlive(S00KeepAlive @in);
    void HandleAnimation(S0BAnimation @in);
    void HandleSpawnMob(S0FSpawnMob @in);
    void HandleJoinGame(S01JoinGame @in);
    void HandlePlayerPosLook(S08PlayerPosLook @in);
    void HandleEntityVelocity(S12EntityVelocity packetIn);
    void HandleEntityMovement(S14Entity packetIn);
    void HandleEntityTeleport(S18EntityTeleport @in);
    void HandleEntityHeadLook(S19EntityHeadLook @in);
    void HandleChunkData(S21ChunkData @in);
    void HandleMapChunkBulk(S26MapChunkBulk @in);
    void HandleMultiBlockChange(S22MultiBlockChange @in);
    void HandleBlockChange(S23BlockChange @in);
    void HandleConfirmTransaction(S32ConfirmTransaction @in);
    void HandlePlayerAbilities(S39PlayerAbilities @in);
    void HandleDisconnect(S40Disconnect @in);
}