using System;
using TestClient.Source.Physics;
using TestClient.Source.Render.Model;
using TestClient.Source.Utility;

namespace TestClient.Source.World.Entities;

public abstract class ServerEntity : Entity
{
    protected ModelRenderer Renderer;
    private int _smoothPosCount;
    private double _nextPosX;
    private double _nextPosY;
    private double _nextPosZ;
    private double _nextRotationYaw;
    private double _nextRotationPitch;

    public ServerEntity(Level level) : base(level)
    {
    }

    protected override void Initialize()
    {
        Game.Singleton.NewEntityNode(this, GetModelRenderer());
    }

    public override void SetPosAndRot2(double x, double y, double z, float yaw, float pitch,
        int posRotationIncrements, bool p_180426_10_)
    {
        _nextPosX = x;
        _nextPosY = y;
        _nextPosZ = z;
        _nextRotationYaw = yaw;
        _nextRotationPitch = pitch;
        _smoothPosCount = posRotationIncrements;
    }
    
    public override void LivingTick()
    {
        base.LivingTick();

        if (_smoothPosCount > 0)
        {
            var f0 = PosX + (_nextPosX - PosX) / _smoothPosCount;
            var f1 = PosY + (_nextPosY - PosY) / _smoothPosCount;
            var d2 = PosZ + (_nextPosZ - PosZ) / _smoothPosCount;
            double d3 = Mth.WrapAngle((float)_nextRotationYaw - RotY);
            RotY = (float)(RotY + d3 / _smoothPosCount);
            RotX = (float)(RotX + (_nextRotationPitch - RotX) / _smoothPosCount);
            --_smoothPosCount;
            SetPos(f0, f1, d2);
            SetRot(RotY, RotX);
        }
        
        PrevLimbSwingAmount = LimbSwingAmount;

        var d0 = PosX - PrevX;
        var d1 = PosZ - PrevZ;
        var f = (float)Math.Sqrt(d0 * d0 + d1 * d1) * 4.0F;

        if (f > 1.0F) f = 1.0F;

        LimbSwingAmount += (f - LimbSwingAmount) * 0.4F;
        LimbSwing += LimbSwingAmount;
    }

    public override void Render(float a)
    {
        var f = Mth.InterpolateRotation(PrevRotYBody, RotYBody, a);
        var f1 = Mth.InterpolateRotation(PrevRotYHead, RotYHead, a);
        var f2 = f1 - f;
        var f7 = PrevRotX + (RotX - PrevRotX) * a;

        var f5 = PrevLimbSwingAmount + (LimbSwingAmount - PrevLimbSwingAmount) * a;
        var f6 = LimbSwing - LimbSwingAmount * (1.0F - a);
        if (IsChild()) f6 *= 3.0F;
        if (f5 > 1.0F) f5 = 1.0F;

        var f4 = 0.0625F;
        
        GetModelRenderer().Update(f6, f5, TicksExisted + a, -f2, f7, f4, this);
    }

    protected T GetModel<T>() where T : EntityModel
    {
        return (T)GetModelRenderer().Model;
    }

    protected abstract ModelRenderer GetModelRenderer();
}