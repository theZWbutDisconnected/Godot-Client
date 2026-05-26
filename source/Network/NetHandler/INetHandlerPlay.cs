using TestClient.Source.Network.Packet.Server.Play;

namespace TestClient.Source.Network.NetHandler;

public interface INetHandlerPlayClient : INetHandler
{
    void HandleKeepAlive(ServerboundKeepAlive @in);
    void HandleAnimation(ServerboundAnimation @in);
    void HandleSpawnMob(ServerboundSpawnMob @in);
    void HandleJoinGame(ServerboundJoinGame @in);
    void HandlePlayerPosLook(ServerboundTeleport @in);
    void HandleEntityVelocity(ServerboundVelocity packetIn);
    void HandleEntityMovement(ServerboundEntityStatus packetIn);
    void HandleEntityTeleport(ServerboundEntityTeleport @in);
    void HandleEntityHeadLook(ServerboundHeadLook @in);
    void HandleChunkData(ServerboundChunkData @in);
    void HandleMapChunkBulk(ServerboundMapChunkBulk @in);
    void HandleMultiBlockChange(ServerboundMultiBlockChange @in);
    void HandleBlockChange(ServerboundBlockChange @in);
    void HandleConfirmTransaction(ServerboundConfirmTransaction @in);
    void HandlePlayerAbilities(ServerboundAbilities @in);
    void HandleDisconnect(ServerboundDisconnect @in);
    void HandleSpawnPlayer(ServerboundSpawnPlayer @in);
}