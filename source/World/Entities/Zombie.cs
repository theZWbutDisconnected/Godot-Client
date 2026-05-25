using System;
using Godot;
using TestClient.Source.Physics;
using TestClient.Source.Render.Model;
using TestClient.Source.Render.Model.impl;

namespace TestClient.Source.World.Entities;

public class Zombie : Entity
{
    private readonly ZombieModel _model = new();
    private readonly ModelRenderer _renderer;
    private int _smoothPosCount;
    private double _nextPosX;
    private double _nextPosY;
    private double _nextPosZ;
    private double _nextRotationYaw;
    private double _nextRotationPitch;
    private int _rotationTickCounter;

    public Zombie(Level level) : base(level)
    {
        Game.Singleton.NewEntityNode(this,
            _renderer = new ModelRenderer(_model, "res://assets/entity/zombie.png", Game.Singleton, 64, 64));
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
        _renderer.Update(f6, f5, TicksExisted + a, f2, f7, f4, this);
    }

    protected override float UpdateDistance(float p_110146_1_, float p_110146_2_)
    {
        UpdateRenderAngles();
        return p_110146_2_;
    }

    public void UpdateRenderAngles()
    {
        var d0 = PosX - PrevX;
        var d1 = PosZ - PrevZ;

        if (d0 * d0 + d1 * d1 > 2.500000277905201E-7D)
        {
            RotYBody = RotY;
            RotYHead = ComputeAngleWithBound(RotYBody, RotYHead, 75.0F);
            PrevRotYHead = RotYHead;
            _rotationTickCounter = 0;
        }
        else
        {
            var f = 75.0F;

            if (Math.Abs(RotYHead - PrevRotYHead) > 15.0F)
            {
                _rotationTickCounter = 0;
                PrevRotYHead = RotYHead;
            }
            else
            {
                ++_rotationTickCounter;
                var i = 10;

                if (_rotationTickCounter > 10) f = Math.Max(1.0F - (_rotationTickCounter - 10) / 10.0F, 0.0F) * 75.0F;
            }

            RotYBody = ComputeAngleWithBound(RotYHead, RotYBody, f);
        }
    }

    private float ComputeAngleWithBound(float p_75665_1_, float p_75665_2_, float p_75665_3_)
    {
        var f = Mth.WrapAngle(p_75665_1_ - p_75665_2_);

        if (f < -p_75665_3_) f = -p_75665_3_;

        if (f >= p_75665_3_) f = p_75665_3_;

        return p_75665_1_ - f;
    }

    private bool IsChild()
    {
        return false;
    }
}