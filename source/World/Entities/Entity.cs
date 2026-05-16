using System;
using Godot;
using TestClient.Source.Physics;

namespace TestClient.Source.World.Entities;

public partial class Entity : Node3D
{
    public AABB BoundingBox;
    public int EntityId = new Random().Next();
    public Guid EntityUuid = Guid.NewGuid();
    public float EyeHeight;
    protected Level Level;
    public bool OnGround;
    public bool Removed;
    public double PosX;
    public double PosY;
    public double PosZ;
    public float PrevRotX;
    public float PrevRotY;
    public double PrevX;
    public double PrevY;
    public double PrevZ;
    public float RotX;
    public float RotY;
    public int ServerX;
    public int ServerY;
    public int ServerZ;
    public double XDelta;
    public double YDelta;
    public double ZDelta;
    protected float Width = 0.6F;
    public float Height = 1.8F;

    public Entity(Level level)
    {
        Level = level;
        SetSize(0.6F, 1.8F);
        SetPos(0.0F, 0.0F, 0.0F);
        EyeHeight = 1.62F;
    }

    protected void SetSize(float w, float h)
    {
        Width = w;
        Height = h;
    }

    public void SetPos(double x, double y, double z)
    {
        PosX = x;
        PosY = y;
        PosZ = z;
        var w = Width / 2.0F;
        var h = Height;
        BoundingBox = new AABB(x - w, y, z - w, x + w, y + h, z + w);
    }

    public void SetRot(float yaw, float pitch)
    {
        RotY = yaw % 360.0F;
        RotX = pitch % 360.0F;
    }

    public void SetPosAndRot(double x, double y, double z, float yaw, float pitch)
    {
        PrevX = PosX = x;
        PrevY = PosY = y;
        PrevZ = PosZ = z;
        PrevRotY = RotY = yaw;
        PrevRotX = RotX = pitch;
        double d0 = PrevRotY - yaw;

        if (d0 < -180.0D) PrevRotY += 360.0F;

        if (d0 >= 180.0D) PrevRotY -= 360.0F;

        SetPos(x, y, z);
        SetRot(yaw, pitch);
    }

    public void SetPosAndRot2(double x, double y, double z, float yaw, float pitch, int posRotationIncrements,
        bool p_180426_10_)
    {
        SetPosAndRot(x, y, z, yaw, pitch);
        var list = Level.GetCubes(BoundingBox.Expand(0.03125F, 0.0F, 0.03125F));

        if (list.Count > 0)
        {
            double d0 = 0.0F;

            foreach (var axisalignedbb in list)
                if (axisalignedbb.Y1 > d0)
                    d0 = axisalignedbb.Y1;

            y += d0 - BoundingBox.Y0;
            SetPos(x, y, z);
        }
    }

    public void Turn(float xo, float yo)
    {
        var f = RotX;
        var f1 = RotY;
        RotY = (float)(RotY + xo * 0.15D);
        RotX = (float)(RotX - yo * 0.15D);
        RotX = Mathf.Clamp(RotX, -90.0F, 90.0F);
        PrevRotX += RotX - f;
        PrevRotY += RotY - f1;
    }

    public virtual void Tick()
    {
        PrevX = PosX;
        PrevY = PosY;
        PrevZ = PosZ;
        PrevRotY = RotY;
        PrevRotX = RotX;
    }

    public void Move(double xa, double ya, double za)
    {
        double xaOrg = xa, yaOrg = ya, zaOrg = za;

        var bb = BoundingBox;
        var aABBs = Level.GetCubes(bb.Expand(xa, ya, za));

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

        PosX = (bb.X0 + bb.X1) * 0.5F;
        PosY = bb.Y0;
        PosZ = (bb.Z0 + bb.Z1) * 0.5F;
    }

    public void MoveRelative(double xa, double za, double speed)
    {
        var dist = xa * xa + za * za;
        if (dist >= 0.01F)
        {
            dist = speed / Math.Sqrt(dist);
            xa *= dist;
            za *= dist;

            var rad = RotY * (float)(Math.PI / 180.0);
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