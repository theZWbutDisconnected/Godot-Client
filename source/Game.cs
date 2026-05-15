using System.Threading.Tasks;
using Godot;
using TestClient.Source.World.Entities;
using TestClient.Source.Network;
using TestClient.Source.Network.NetHandler.impl;
using TestClient.Source.Network.Packet.Client.Handshake;
using TestClient.Source.Network.Packet.Client.Login;
using TestClient.Source.Physics;
using TestClient.Source.Render;
using TestClient.Source.World;
using TestClient.Source.World.Tile;

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

	private bool _isRunning;
	
	public static Game Singleton { get; private set;  }

	public Game()
	{
		Singleton = this;
	}

	public override async void _Ready()
	{
		await NetworkInitialize();
		_isRunning = true;
		Level = new Level();
		Player = new Player(Level, _network);
		Level.AddEntity(Player);
		
		AddChild(Level);
		AddChild(Player);
		
		Input.SetMouseMode(Input.MouseModeEnum.Captured);
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
		if (!_isRunning) return;
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
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			Level.SetBlock(new BlockPos(Player.X, Player.Y - 1, Player.Z), 7, 0);
		}
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
		var x = Player.PrevX + (Player.X - Player.PrevX) * a;
		var y = Player.PrevY + (Player.Y - Player.PrevY) * a;
		var z = Player.PrevZ + (Player.Z - Player.PrevZ) * a;
		Camera.Position = new Vector3((float)x, (float)y + Player.EyeHeight, (float)z);
		Camera.RotationDegrees = new Vector3(-Player.Pitch, 180 - Player.Yaw, 0.0F);
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (!_isRunning) return;
		if (@event is InputEventMouseMotion motion)
		{
			var xo = 0.0F;
			var yo = 0.0F;
			xo = motion.Relative.X;
			yo = -motion.Relative.Y;

			var YMouseAxis = 1;
			Player.Turn(xo, yo * YMouseAxis);
		}
	}
}
