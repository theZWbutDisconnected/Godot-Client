using System;
using Godot;
using TestClient.Source.Network.Packet.Client.Login;
using TestClient.Source.Network.Packet.NetHandler;
using TestClient.Source.Network.Packet.Server.Login;

namespace TestClient.Source.Network.Packet.Client;

public class NetHandlerLoginClient : INetHandlerLoginClient
{
    private readonly NetworkSystem _networkSystem;
    private readonly string _username;

    public NetHandlerLoginClient(NetworkSystem networkSystem, string username)
    {
        _networkSystem = networkSystem;
        _username = username;
    }

    public void HandleEncryptionRequest(S01PacketEncryptionRequest packetIn)
    {
        GD.Print("Sending empty response");

        var response = new C01PacketEncryptionResponse(
            Array.Empty<byte>(),
            Array.Empty<byte>()
        );
        _networkSystem.SendPacket(response);
    }

    public void HandleLoginSuccess(S02PacketLoginSuccess packetIn)
    {
        GD.Print($"UUID: {packetIn.Uuid}, Username: {packetIn.Username}");
        _networkSystem.SetUsername(packetIn.Username);
        _networkSystem.SetState(ConnectionState.Play);
    }

    public void HandleDisconnect(S00Disconnect packetIn)
    {
        GD.PrintErr($"Disconnected during login: {packetIn.Reason}");
        _networkSystem.Disconnect();
    }

    public void HandleEnableCompression(S03PacketEnableCompression packetIn)
    {
        _networkSystem.SetCompressionThreshold(packetIn.Threshold);
    }

    public void Disconnected(string reason)
    {
        GD.PrintErr($"Login connection lost: {reason}");
    }
}