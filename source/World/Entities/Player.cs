using System;
using Godot;
using TestClient.Source.Network;
using TestClient.Source.Network.Packet.Client.Play;
using TestClient.Source.Physics;

namespace TestClient.Source.World.Entities;

public class Player : Entity
{
    public readonly NetworkSystem SendQueue;
    private int _positionUpdateTicks;
    public float LastPitch;
    public double LastX;
    public double LastY;
    public float LastYaw;
    public double LastZ;
    public float EquipProgress;
    
    public Capabilities Capabilities = new();

    public Player(Level level, NetworkSystem netHandler) : base(level)
    {
        SendQueue = netHandler;
    }

    public override void Swing()
    {
        base.Swing();
        SendQueue.SendPacket(new ClientboundAnimation());
    }

    public override void Tick()
    {
        if (Level.IsBlockLoaded(new BlockPos(PosX, 0.0D, PosZ)))
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

            var dx0 = PosX - PrevX;
            var dz0 = PosZ - PrevZ;
            var limbSpeed = (float)Mathf.Sqrt(dx0 * dx0 + dz0 * dz0) * 4.0F;
            if (limbSpeed > 1.0F) limbSpeed = 1.0F;
            PrevLimbSwingAmount = LimbSwingAmount;
            LimbSwingAmount += (limbSpeed - LimbSwingAmount) * 0.4F;
            LimbSwing += LimbSwingAmount;

            if (SendQueue.IsConnected())
            {
                var delta0 = PosX - LastX;
                var delta1 = BoundingBox.Y0 - LastY;
                var delta2 = PosZ - LastZ;
                var delta3 = (double)(RotY - LastYaw);
                var delta4 = (double)(RotX - LastPitch);
                var sendThreshold = 9.0E-4D;
                var posReported = Mth.LengthSquared(delta0, delta1, delta2) > sendThreshold ||
                                  _positionUpdateTicks >= 20;
                var rotReported = delta3 != 0.0D || delta4 != 0.0D;

                if (posReported && rotReported)
                    SendQueue.SendPacket(new ClientboundPlayerPosLook(PosX, BoundingBox.Y0, PosZ, RotY, RotX, OnGround));
                else if (posReported) SendQueue.SendPacket(new ClientboundPlayerMove(PosX, BoundingBox.Y0, PosZ, OnGround));
                else if (rotReported) SendQueue.SendPacket(new ClientboundPlayerLook(RotY, RotX, OnGround));
                else SendQueue.SendPacket(new ClientboundPlayerStatus(OnGround));

                ++_positionUpdateTicks;
                if (posReported)
                {
                    LastX = PosX;
                    LastY = BoundingBox.Y0;
                    LastZ = PosZ;
                    _positionUpdateTicks = 0;
                }

                if (rotReported)
                {
                    LastYaw = RotY;
                    LastPitch = RotX;
                }
            }
        }
    }

    public void SendAbilities()
    {
        SendQueue.SendPacket(new ClientboundAbilities(Capabilities));
    }
}
