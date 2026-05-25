using System;
using Godot;
using TestClient.Source.Physics;
using TestClient.Source.Utility;

namespace TestClient.Source.World.Entities;

public class Entity
{
    public AABB BoundingBox;
    public int EntityId = new Random().Next();
    public Guid EntityUuid = Guid.NewGuid();
    public float EyeHeight;
    public float Height = 1.8F;
    protected Level Level;
    public float LimbSwing;
    public float LimbSwingAmount;
    protected float MovedDistance;
    protected float PrevMovedDistance;
    public bool OnGround;
    public double PosX;
    public double PosY;
    public double PosZ;
    public double PrevX;
    public double PrevY;
    public double PrevZ;
    public float RotX;
    public float RotY;
    public float RotYBody;
    public float RotYHead;
    public float PrevRotX;
    public float PrevRotY;
    public float PrevRotYBody;
    public float PrevRotYHead;
    public float PrevLimbSwingAmount;
    public float PrevSwingProgress;
    public bool Removed;
    public int ServerX;
    public int ServerY;
    public int ServerZ;
    public float SwingProgress;
    public float TicksExisted;
    protected float Width = 0.6F;
    public double XDelta;
    public double YDelta;
    public double ZDelta;
    public int HurtTime;
    private bool _isSwingInProgress;
    private int _swingProgressInt;

    public DataWatcher DataWatcher { get; private set; }

    public Entity(Level level)
    {
        Level = level;
        SetSize(0.6F, 1.8F);
        SetPos(0.0F, 0.0F, 0.0F);
        EyeHeight = 1.62F;
        DataWatcher = new DataWatcher(this);
        DataWatcher.AddObject(0, (byte)0);
        DataWatcher.AddObject(1, (short)300);
        DataWatcher.AddObject(3, (byte)0);
        DataWatcher.AddObject(2, "");
        DataWatcher.AddObject(4, (byte)0);
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

    public void Remove()
    {
        Removed = true;
    }

    public virtual void SetPosAndRot(double x, double y, double z, float yaw, float pitch)
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

    public virtual void SetPosAndRot2(double x, double y, double z, float yaw, float pitch, int posRotationIncrements,
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
        RotY += xo * 0.15F;
        RotX -= yo * 0.15F;
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
        PrevRotYBody = RotYBody;
        PrevRotYHead = RotYHead;
        PrevMovedDistance = MovedDistance;
        PrevSwingProgress = SwingProgress;

        LivingTick();

        var d0 = PosX - PrevX;
        var d1 = PosZ - PrevZ;
        var f = d0 * d0 + d1 * d1;
        var f1 = RotYBody;
        var f2 = 0.0F;
        if (f > 0.0025000002F)
        {
            f2 = Mathf.Sqrt((float)f) * 3.0F;
            f1 = Mathf.Atan2((float)d1, (float)d0) * 180.0F / MathF.PI - 90.0F;
        }
        if (SwingProgress > 0.0F) f1 = RotY;
        f2 = UpdateDistance(f1, f2);
        
        while (RotY - PrevRotY < -180.0F) PrevRotY -= 360.0F;
        while (RotY - PrevRotY >= 180.0F) PrevRotY += 360.0F;
        while (RotYBody - PrevRotYBody < -180.0F) PrevRotYBody -= 360.0F;
        while (RotYBody - PrevRotYBody >= 180.0F) PrevRotYBody += 360.0F;
        while (RotX - PrevRotX < -180.0F) PrevRotX -= 360.0F;
        while (RotX - PrevRotX >= 180.0F) PrevRotX += 360.0F;
        while (RotYHead - PrevRotYHead < -180.0F) PrevRotYHead -= 360.0F;
        while (RotYHead - PrevRotYHead >= 180.0F) PrevRotYHead += 360.0F;
        
        MovedDistance += f2;
    }

    public virtual void LivingTick()
    {
        --HurtTime;
        UpdateSwing();
    }

    public virtual void Render(float a)
    {
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

            var rad = RotY * (Math.PI / 180.0);
            var sin = Math.Sin(rad);
            var cos = Math.Cos(rad);

            XDelta -= xa * cos - za * sin;
            ZDelta -= za * cos + xa * sin;
        }
    }

    public void SetVelocity(double x, double y, double z)
    {
        XDelta = x;
        YDelta = y;
        ZDelta = z;
    }

    protected virtual float UpdateDistance(float p_110146_1_, float p_110146_2_)
    {
        var f = Mth.WrapAngle(p_110146_1_ - RotYBody);
        RotYBody += f * 0.3F;
        var f1 = Mth.WrapAngle(RotY - RotYBody);
        var flag = f1 < -90.0F || f1 >= 90.0F;
        if (f1 < -75.0F) f1 = -75.0F;
        if (f1 >= 75.0F) f1 = 75.0F;
        RotYBody = RotY - f1;
        if (f1 * f1 > 2500.0F) RotYBody += f1 * 0.2F;
        if (flag) p_110146_2_ *= -1.0F;
        return p_110146_2_;
    }

    public virtual void SetHeadYaw(float f)
    {
        RotYHead = f;
    }

    public void OnDataWatcherUpdate(int dataID)
    {
    }

    public virtual void HurtAnimation()
    {
        HurtTime = 10;
    }
    
    protected virtual void UpdateSwing()
    {
        int i = SwingDuration();

        if (_isSwingInProgress)
        {
            ++_swingProgressInt;

            if (_swingProgressInt >= i)
            {
                _swingProgressInt = 0;
                _isSwingInProgress = false;
            }
        }
        else
        {
            _swingProgressInt = 0;
        }

        SwingProgress = _swingProgressInt / (float)i;
    }

    public virtual void Swing()
    {
        if (!_isSwingInProgress || _swingProgressInt >= SwingDuration() / 2 || _swingProgressInt < 0)
        {
            _swingProgressInt = -1;
            _isSwingInProgress = true;
        }
    }
    
    private int SwingDuration()
    {
        return 6;
    }

    public float GetSwingProgress(float partialTickTime)
    {
        float f = SwingProgress - PrevSwingProgress;

        if (f < 0.0F)
        {
            ++f;
        }

        return PrevSwingProgress + f * partialTickTime;
    }

    public bool IsRiding()
    {
        return false;
    }
}