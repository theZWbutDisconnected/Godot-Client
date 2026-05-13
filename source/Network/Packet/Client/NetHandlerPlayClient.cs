using System.IO;
using Godot;
using TestClient.Source.Network.Packet.Client.Play;
using TestClient.Source.Network.Packet.NetHandler;
using TestClient.Source.Network.Packet.Server.Play;

namespace TestClient.Source.Network.Packet.Client;

public class NetHandlerPlayClient : INetHandlerPlayClient
{
    private readonly NetworkSystem _networkSystem;
    private readonly string _username;

    public NetHandlerPlayClient(NetworkSystem networkSystem, string username)
    {
        _networkSystem = networkSystem;
        _username = username;
    }

    public void HandleKeepAlive(S00KeepAlive packetIn)
    {
        _networkSystem.SendPacket(new C00KeepAlive(packetIn.KeepAliveId));
    }

    public async void HandleJoinGame(S01JoinGame packetIn)
    {
        var brandBuf = new PacketBuffer(new MemoryStream());
        brandBuf.WriteString("vanilla");
        await _networkSystem.SendPacket(new C17PacketCustomPayload("MC|Brand", brandBuf));
        GD.Print($"EntityId={packetIn.EntityId}, GameType={packetIn.GameType}, " +
                 $"Dimension={packetIn.Dimension}, Difficulty={packetIn.Difficulty}, " +
                 $"MaxPlayers={packetIn.MaxPlayers}, WorldType={packetIn.WorldType}");
    }

    public void Disconnected(string reason)
    {
        GD.PrintErr($"Play connection lost: {reason}");
    }
}