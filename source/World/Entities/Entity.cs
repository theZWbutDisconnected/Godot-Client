using System;
using System.Collections.Generic;
using Godot;
using TestClient.Source.Physics;

namespace TestClient.Source.World.Entities;

public partial class Entity : Node3D
{
    protected Level Level;
    public bool OnGround;
    public bool Removed;
    public double X;
    public double Y;
    public double Z;
    public double PrevX;
    public double PrevY;
    public double PrevZ;
    public int ServerX;
    public int ServerY;
    public int ServerZ;
    public double XDelta;
    public double YDelta;
    public double ZDelta;
    public float Yaw;
    public float Pitch;
    public float PrevYaw;
    public float PrevPitch;
    public float EyeHeight;
    public int EntityId = new Random().Next();
    public Guid EntityUuid = Guid.NewGuid();
    public AABB BoundingBox;
    protected float Width = 0.6F;
    public float Height = 1.8F;

    public Entity(Level level) {
        Level = level;
        SetSize(0.6F, 1.8F);
        SetPos(0.0F, 0.0F, 0.0F);
    }

    protected void SetSize(float w, float h)
    {
        Width = w;
        Height = h;
        EyeHeight = h * 0.86F;
    }

    public void SetPos(double x, double y, double z) {
        X = x;
        Y = y;
        Z = z;
        float w = Width / 2.0F;
        float h = Height;
        BoundingBox = new AABB(x - w, y, z - w, x + w, y + h, z + w);
    }

    public void SetRot(float yaw, float pitch)
    {
        Yaw = yaw % 360.0F;
        Pitch = pitch % 360.0F;
    }

    public void SetPosAndRot(double x, double y, double z, float yaw, float pitch)
    {
        PrevX = X = x;
        PrevY = Y = y;
        PrevZ = Z = z;
        PrevYaw = Yaw = yaw;
        PrevPitch = Pitch = pitch;
        double d0 = PrevYaw - yaw;

        if (d0 < -180.0D)
        {
            PrevYaw += 360.0F;
        }

        if (d0 >= 180.0D)
        {
            PrevYaw -= 360.0F;
        }

        SetPos(x, y, z);
        SetRot(yaw, pitch);
    }

    public void SetPosAndRot2(double x, double y, double z, float yaw, float pitch, int posRotationIncrements, bool p_180426_10_)
    {
        SetPosAndRot(x, y, z, yaw, pitch);
        List<AABB> list = Level.GetCubes(BoundingBox.Expand(0.03125F, 0.0F, 0.03125F));

        if (list.Count > 0)
        {
            double d0 = 0.0F;

            foreach (AABB axisalignedbb in list)
            {
                if (axisalignedbb.Y1 > d0)
                {
                    d0 = axisalignedbb.Y1;
                }
            }

            y += d0 - BoundingBox.Y0;
            SetPos(x, y, z);
        }
    }

    public void Turn(float xo, float yo)
    {
        var f = Pitch;
        var f1 = Yaw;
        Yaw = (float)(Yaw + xo * 0.15D);
        Pitch = (float)(Pitch - yo * 0.15D);
        Pitch = Mathf.Clamp(Pitch, -90.0F, 90.0F);
        PrevPitch += Pitch - f;
        PrevYaw += Yaw - f1;
    }

    public virtual void Tick()
    {
        PrevX = X;
        PrevY = Y;
        PrevZ = Z;
        PrevYaw = Yaw;
        PrevPitch = Pitch;
    }

    public void Move(double xa, double ya, double za)
    {
        double xaOrg = xa, yaOrg = ya, zaOrg = za;

        var bb = BoundingBox;
        List<AABB> aABBs = Level.GetCubes(bb.Expand(xa, ya, za));

        for (var i = 0; i < aABBs.Count; i++)
            ya = aABBs[i].ClipYCollide(bb, ya);
        bb.Move(0.0F, ya, 0.0F);

        for (var i = 0; i < aABBs.Count; i++)
            xa = aABBs[i].ClipXCollide(bb, xa);
        bb.Move(xa, 0.0F, 0.0F);

        for (var i = 0; i < aABBs.Count; i++)
            za = aABBs[i].ClipZCollide(bb, za);
        bb.Move(0.0F, 0.0F, za);

        OnGround = yaOrg != ya && yaOrg < 0.0F;

        if (xaOrg != xa) XDelta = 0.0F;
        if (yaOrg != ya) YDelta = 0.0F;
        if (zaOrg != za) ZDelta = 0.0F;

        X = (bb.X0 + bb.X1) * 0.5F;
        Y = bb.Y0;
        Z = (bb.Z0 + bb.Z1) * 0.5F;
    }

    public void MoveRelative(double xa, double za, double speed)
    {
        var dist = xa * xa + za * za;
        if (dist >= 0.01F)
        {
            dist = speed / Math.Sqrt(dist);
            xa *= dist;
            za *= dist;

            var rad = Yaw * (float)(Math.PI / 180.0);
            var sin = (float)Math.Sin(rad);
            var cos = (float)Math.Cos(rad);

            XDelta -= xa * cos - za * sin;
            ZDelta -= za * cos + xa * sin;
        }
    }

    public virtual void SetHeadYaw(float f)
    {
    }
}