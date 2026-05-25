using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using TestClient.Source.Network;
using TestClient.Source.Network.NetHandler.impl;
using TestClient.Source.Network.Packet.Client.Handshake;
using TestClient.Source.Network.Packet.Client.Login;
using TestClient.Source.Physics;
using TestClient.Source.Render.Model;
using TestClient.Source.World;
using TestClient.Source.World.Entities;

namespace TestClient.Source;

public partial class Game : Node3D
{
	private const string ip = "127.0.0.1";
	private const int port = 25565;
	private const string username = "LocalPlayer";

	private readonly NetworkSystem _network = new();
	private readonly Timer _timer = new(20);

	private string _fpsString = "0 fps";
	private int _frames;

	private bool _isRunning;
	private long _lastTime = (long)Time.GetTicksMsec();

	[Export] public Camera3D Camera;
	public Level Level;
	public Player Player;

	private readonly Dictionary<Entity, ModelRenderer> _entityModels = [];

	public Game()
	{
		Singleton = this;
	}

	public static Game Singleton { get; private set; }

	public override async void _Ready()
	{
		await NetworkInitialize();
		_isRunning = true;
		Level = new Level();
		Player = new Player(Level, _network);
		Level.AddEntity(Player);
		AddChild(Level);
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
		Level.Tick();
	}

	private void Render(float alpha)
	{
		SetupCamera(alpha);
		foreach (var i in _entityModels)
		{
			var entity = i.Key;
			var model = i.Value.Root;
			if (!entity.Removed)
			{
				entity.Render(alpha);
				var f4 = 0.0625F;
				model.Scale = new Vector3(-f4, -f4, f4);
				model.Position = new Vector3((float)(entity.PrevX + (entity.PosX - entity.PrevX) * alpha), (float)(entity.PrevY + (entity.PosY - entity.PrevY) * alpha), (float)(entity.PrevZ + (entity.PosZ - entity.PrevZ) * alpha));
				model.Position += new Vector3(0.0F, 24F / 16F, 0.0F);
				model.RotationDegrees = new Vector3(0.0F, 180 - entity.RotYBody, 0.0F);
				if (!model.IsInsideTree()) AddChild(model);
			}
			else
			{
				i.Value.Free();
			}
		}
	}

	public void NewEntityNode(Entity entity, ModelRenderer renderer)
	{
		_entityModels.Add(entity, renderer);
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
		var x = Player.PrevX + (Player.PosX - Player.PrevX) * a;
		var y = Player.PrevY + (Player.PosY - Player.PrevY) * a;
		var z = Player.PrevZ + (Player.PosZ - Player.PrevZ) * a;
		Camera.Position = new Vector3((float)x, (float)y + Player.EyeHeight, (float)z);
		Camera.RotationDegrees = new Vector3(-Player.RotX, 180 - Player.RotY, 0.0F);
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

		if (@event is InputEventKey key)
		{
			if (key.Pressed && key.Keycode == Key.G)
			{
				var zombie = new Zombie(Level);
				Level.AddEntity(zombie);
			}
		}
	}
}
