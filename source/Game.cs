using Godot;
using TestClient.Source.Network;
using TestClient.Source.Network.Packet.Client;
using TestClient.Source.Network.Packet.Client.Handshake;
using TestClient.Source.Network.Packet.Client.Login;

namespace TestClient.Source;

public partial class Game : Node
{
    private readonly NetworkSystem _networkSystem = new();

    private string ip = "127.0.0.1";
    private short port = 25565;
    private string username = "LocalPlayer";

    public override async void _Ready()
    {
        _networkSystem.SetUsername(username);
        await _networkSystem.Connect(ip, port);
        _networkSystem.SetState(ConnectionState.HandShaking).SetHandler(new NetHandlerHandshakeTcp());
        await _networkSystem.SendPacket(new C00Handshake(47, ip, port, ConnectionState.Login));
        _networkSystem.SetState(ConnectionState.Login).SetHandler(new NetHandlerLoginClient(_networkSystem));
        await _networkSystem.SendPacket(new C00LoginStart(username));
    }

    public override void _Process(double delta)
    {
        _networkSystem.StreamProcess();
    }
}