using System;
using Godot;
using TestClient.Source.Network;
using TestClient.Source.Network.NetHandler.impl;
using TestClient.Source.Network.Packet.Client.Play;

namespace TestClient.Source.World.Entity;

public partial class Player : Entity
{
    public NetworkSystem SendQueue;
    public float LastX;
    public float LastY;
    public float LastZ;
    public float LastYaw;
    public float LastPitch;
    private int _positionUpdateTicks;
    
    public Player(Level level, NetworkSystem netHandler) : base(level)
    {
        SendQueue = netHandler;
    }

    public override void Tick()
    {
        base.Tick();

        var xa = 0.0F;
        var ya = 0.0F;

        if (Input.IsKeyPressed(Key.Up) || Input.IsKeyPressed(Key.W)) --ya;
        if (Input.IsKeyPressed(Key.Down) || Input.IsKeyPressed(Key.S)) ++ya;
        if (Input.IsKeyPressed(Key.Left) || Input.IsKeyPressed(Key.A)) --xa;
        if (Input.IsKeyPressed(Key.Right) || Input.IsKeyPressed(Key.D)) ++xa;
        if ((Input.IsKeyPressed(Key.Space) || Input.IsKeyPressed(Key.Bracketright)) && OnGround) YDelta = 0.5F;

        MoveRelative(xa, ya, OnGround ? 0.1F : 0.02F);
        YDelta = (float)(YDelta - 0.08);
        Move((float)XDelta, (float)YDelta, (float)ZDelta);
        XDelta *= 0.91F;
        YDelta *= 0.98F;
        ZDelta *= 0.91F;

        if (OnGround)
        {
            XDelta *= 0.7F;
            ZDelta *= 0.7F;
        }

        if (SendQueue.IsConnected())
        {
            bool posReported = _positionUpdateTicks >= 20;
            if (posReported)
            {
                SendQueue.SendPacket(new C04PlayerPosition(X, BoundingBox.Y0, Z, OnGround));
            }
            else
            {
                SendQueue.SendPacket(new C03Player(OnGround));
            }
            ++_positionUpdateTicks;
            if (posReported)
            {
                LastX = X;
                LastY = BoundingBox.Y0;
                LastZ = Z;
                _positionUpdateTicks = 0;
            }
        }
    }
}