using System;
using Godot;
using TestClient.Source.Network.Packet.Client.Login;
using TestClient.Source.Network.Packet.NetHandler;
using TestClient.Source.Network.Packet.Server.Login;

namespace TestClient.Source.Network.NetHandler.impl;

public class NetHandlerLoginClient : INetHandlerLoginClient
{
    private readonly NetworkSystem _networkSystem;
    private readonly string _username;

    public NetHandlerLoginClient(NetworkSystem networkSystem)
    {
        _networkSystem = networkSystem;
        _username = networkSystem.Username;
    }

    public void HandleEncryptionRequest(S01EncryptionRequest @in)
    {
        GD.Print("Sending empty response");

        var response = new C01EncryptionResponse(
            Array.Empty<byte>(),
            Array.Empty<byte>()
        );
        _networkSystem.SendPacket(response);
    }

    public void HandleLoginSuccess(S02LoginSuccess @in)
    {
        GD.Print($"UUID: {@in.Uuid}, Username: {@in.Username}");
        _networkSystem.SetUsername(@in.Username);
        _networkSystem.SetState(ConnectionState.Play).SetHandler(new NetHandlerPlayClient(_networkSystem));
    }

    public void HandleDisconnect(S00Disconnect packetIn)
    {
        GD.PrintErr($"Disconnected during login: {packetIn.Reason}");
        _networkSystem.Disconnect();
    }

    public void HandleEnableCompression(S03EnableCompression @in)
    {
        _networkSystem.SetCompressionThreshold(@in.Threshold);
    }

    public void Disconnected(string reason)
    {
        GD.PrintErr($"Login connection lost: {reason}");
    }
}