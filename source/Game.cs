using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using TestClient.Source.Gui;
using TestClient.Source.Network;
using TestClient.Source.Network.NetHandler.impl;
using TestClient.Source.Network.Packet.Client.Handshake;
using TestClient.Source.Network.Packet.Client.Login;
using TestClient.Source.Network.Packet.Client.Play;
using TestClient.Source.Physics;
using TestClient.Source.Render;
using TestClient.Source.Render.Font;
using TestClient.Source.Render.Model;
using TestClient.Source.World;
using TestClient.Source.World.Entities;

namespace TestClient.Source;

public partial class Game : Node3D
{
	private const string ip = "127.0.0.1";
	private const int port = 25565;
	private const string username = "LocalPlayer";

	public Timer Timer { get; } = new(20);
	private readonly NetworkSystem _network = new();

	private string _fpsString = "0 fps";
	private int _frames;

	private bool _isRunning;
	private long _lastTime = (long)Time.GetTicksMsec();

	[Export] public Camera3D Camera;
	public Level Level;
	public Player Player;
	public MoveObject Mop;

	private readonly Dictionary<Entity, ModelRenderer> _entityModels = [];
	private readonly List<Entity> _entityModelsRemoveList = [];
	private FirstPersonArm _armFp;
	private BlockOutline _outline;
	private GuiRenderer _guiRenderer;

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

		_armFp = new FirstPersonArm(this, "res://assets/entity/steve.png");

		_outline = new BlockOutline();
		AddChild(_outline.Node);

		_guiRenderer = new GuiRenderer();
		AddChild(_guiRenderer);

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
		Timer.UpdateTimer();

		DoRaytrace();
		for (var i = 0; i < Timer.ElapsedTicks; ++i) Tick();
		Render(Timer.RenderPartialTicks);
		_armFp.Setup(Camera, delta);
		_outline.Update(Level, Mop);
		++_frames;

		while (Time.GetTicksMsec() >= (ulong)(_lastTime + 1000L))
		{
			_fpsString = _frames + " fps我草泥马";
			_lastTime += 1000L;
			_frames = 0;
		}
	}

	private void DoRaytrace()
	{
		double startX = Player.PosX;
		double startY = Player.PosY + Player.EyeHeight;
		double startZ = Player.PosZ;

		float pitch = (float)(Player.RotX * Math.PI / 180.0);
		float yaw   = (float)(Player.RotY * Math.PI / 180.0);
		double reach = 4.5F;

		double endX = startX - Math.Sin(yaw) * Math.Cos(pitch) * reach;
		double endY = startY - Math.Sin(pitch) * reach;
		double endZ = startZ + Math.Cos(yaw) * Math.Cos(pitch) * reach;

		Mop = Raycast.RayTrace(startX, startY, startZ, endX, endY, endZ, Level, Player);
		
		if (Mop.Type == MoveObjectType.Entity)
		{
			reach = 3.0F;
			endX = startX - Math.Sin(yaw) * Math.Cos(pitch) * reach;
			endY = startY - Math.Sin(pitch) * reach;
			endZ = startZ + Math.Cos(yaw) * Math.Cos(pitch) * reach;
			Mop = Raycast.RayTraceEntities(startX, startY, startZ, endX, endY, endZ, Level, Player);
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

		_armFp.Update(Player, alpha);

		foreach (var i in _entityModels)
		{
			var entity = i.Key;
			var model = i.Value.Root;
			if (!entity.Removed)
			{
				entity.Render(alpha);
				var f4 = 0.0625F;
				var heightOffset = 24F / 16F;
				if (entity.IsChild())
				{
					f4 *= 0.5f;
					heightOffset *= 0.5f;
				}
				if (entity.IsSneaking()) heightOffset *= 0.88f;
				model.Scale = new Vector3(-f4, -f4, f4);
				model.Position = new Vector3((float)(entity.PrevX + (entity.PosX - entity.PrevX) * alpha), (float)(entity.PrevY + (entity.PosY - entity.PrevY) * alpha), (float)(entity.PrevZ + (entity.PosZ - entity.PrevZ) * alpha));
				model.Position += new Vector3(0.0F, heightOffset, 0.0F);
				model.RotationDegrees = new Vector3(0.0F, 180 - (entity.PrevRotYBody + (entity.RotYBody - entity.PrevRotYBody) * alpha), 0.0F);
				if (!model.IsInsideTree()) AddChild(model);
			}
			else
			{
				i.Value.Free();
				_entityModelsRemoveList.Add(entity);
			}
		}

		foreach (var entity in _entityModelsRemoveList)
			_entityModels.Remove(entity);
		_entityModelsRemoveList.Clear();
		
		_guiRenderer.Begin();
		_guiRenderer.DrawString(_fpsString, 0, 0, 0xFFFFFF);
		_guiRenderer.End();
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

	private void ClickMouse()
	{
		if (Mop.Type != MoveObjectType.Entity) Player.Swing();
		switch (Mop.Type)
		{
			case MoveObjectType.Entity:
				var target = Mop.EntityHit;
				Player.Swing();
				_network.SendPacket(new ClientboundUseEntity(target, ClientboundUseEntity.UseEntityAction.Attack));
				break;
		}
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

		if (@event is InputEventMouseButton but)
		{
			if (but.IsPressed()) {
				if (but.ButtonIndex == MouseButton.Left)
				{
					ClickMouse();
				}
			}
		}
	}

	public override void _ExitTree()
	{
		_isRunning = false;
		_network?.Dispose();
		Tessellator.ClearAnimCache();
		_armFp?.Free();
		_outline?.Free();
		_guiRenderer?.Free();
		_entityModelsRemoveList.Clear();
		foreach (var kvp in _entityModels)
			kvp.Value.Free();
		_entityModels.Clear();
	}
}
