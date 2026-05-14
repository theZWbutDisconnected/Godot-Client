using System.Threading.Tasks;
using Godot;
using TestClient.Source.World.Entity;
using TestClient.Source.Network;
using TestClient.Source.Network.NetHandler.impl;
using TestClient.Source.Network.Packet.Client.Handshake;
using TestClient.Source.Network.Packet.Client.Login;
using TestClient.Source.World;

namespace TestClient.Source;

public partial class Game : Node
{
	private const string ip = "127.0.0.1";
	private const short port = 25565;
	private const string username = "LocalPlayer";
	
	[Export] public Camera3D Camera;

	private readonly NetworkSystem _network = new();
	private readonly Timer _timer = new(20);
	public Level Level;
	public Player Player;
	
	private string _fpsString = "0 fps";
	private int _frames;
	private long _lastTime = (long)Time.GetTicksMsec();
	
	public static Game Singleton { get; private set;  }

	public Game()
	{
		Singleton = this;
	}

	public override void _Ready()
	{
		NetworkInitialize();
		Level = new Level();
		Player = new Player(Level, _network);
		AddChild(Level);
		AddChild(Player);
	}

	private async Task NetworkInitialize()
	{
		_network.SetUsername(username);
		await _network.Connect(ip, port);
		_network.SetState(ConnectionState.HandShaking).SetHandler(new NetHandlerHandshakeTcp());
		await _network.SendPacket(new C00Handshake(47, ip, port, ConnectionState.Login));
		_network.SetState(ConnectionState.Login).SetHandler(new NetHandlerLoginClient(_network));
		await _network.SendPacket(new C00LoginStart(username));
	}

	public override void _Process(double delta)
	{
		_timer.UpdateTimer();
		for (var i = 0; i < _timer.ElapsedTicks; ++i) Tick();
		Render(_timer.RenderPartialTicks);
		++_frames;

		while (Time.GetTicksMsec() >= (ulong)(_lastTime + 1000L))
		{
			_fpsString = _frames + " fps";
			_lastTime += 1000L;
			_frames = 0;
		}
	}

	private void Tick()
	{
		if (_network.IsConnected()) _network.StreamProcess();
		Player.Tick();
	}

	private void Render(float alpha)
	{
		SetupCamera(alpha);
	}
	
	private void SetupCamera(float a)
	{
		Camera.Fov = 70.0F;
		Camera.Near = 0.05F;
		Camera.Far = 1000.0F;
		MoveCameraToPlayer(a);
	}

	private void MoveCameraToPlayer(float a)
	{
		float x = Player.PrevX + (Player.X - Player.PrevX) * a;
		float y = Player.PrevY + (Player.Y - Player.PrevY) * a;
		float z = Player.PrevZ + (Player.Z - Player.PrevZ) * a;
		Camera.Position = new Vector3(x, y + Player.EyeHeight, z);
		Camera.RotationDegrees = new Vector3(Player.Pitch, Player.Yaw, 0.0F);
	}
}
