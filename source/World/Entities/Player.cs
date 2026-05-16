using System;
using Godot;
using TestClient.Source.Network;
using TestClient.Source.Network.NetHandler.impl;
using TestClient.Source.Network.Packet.Client.Play;
using TestClient.Source.Physics;

namespace TestClient.Source.World.Entities;

public partial class Player : Entity
{
    public readonly NetworkSystem SendQueue;
    public float HeadYaw;
    public double LastX;
    public double LastY;
    public double LastZ;
    public float LastYaw;
    public float LastPitch;
    private int _positionUpdateTicks;
    
    public Player(Level level, NetworkSystem netHandler) : base(level)
    {
        SendQueue = netHandler;
    }

    public override void SetHeadYaw(float f)
    {
        HeadYaw = f;
    }

    public override void Tick()
    {
        if (Level.IsBlockLoaded(new BlockPos(X, 0.0D, Z)))
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
            YDelta -= 0.08;
            Move(XDelta, YDelta, ZDelta);
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
                double delta0 = X - LastX;
                double delta1 = BoundingBox.Y0 - LastY;
                double delta2 = Z - LastZ;
                var delta3 = (double)(Yaw - LastYaw);
                var delta4 = (double)(Pitch - LastPitch);
                var sendThreshold = 9.0E-4D;
                var posReported = Mth.LengthSquared(delta0, delta1, delta2) > sendThreshold ||
                                  _positionUpdateTicks >= 20;
                var rotReported = delta3 != 0.0D || delta4 != 0.0D;

                if (posReported && rotReported)
                    SendQueue.SendPacket(new C06PlayerPosLook(X, BoundingBox.Y0, Z, Yaw, Pitch, OnGround));
                else if (posReported) SendQueue.SendPacket(new C04PlayerPosition(X, BoundingBox.Y0, Z, OnGround));
                else if (rotReported) SendQueue.SendPacket(new C05PlayerLook(Yaw, Pitch, OnGround));
                else SendQueue.SendPacket(new C03Player(OnGround));

                ++_positionUpdateTicks;
                if (posReported)
                {
                    LastX = X;
                    LastY = BoundingBox.Y0;
                    LastZ = Z;
                    _positionUpdateTicks = 0;
                }

                if (rotReported)
                {
                    LastYaw = Yaw;
                    LastPitch = Pitch;
                }
            }
        }
    }
}