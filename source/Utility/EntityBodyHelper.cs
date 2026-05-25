using System;
using TestClient.Source.Physics;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Utility;

public class EntityBodyHelper(Entity owner)
{
    private int _rotationTickCounter;
    private float _prevRotYHead;
    
    public void UpdateRenderAngles()
    {
        var d0 = owner.PosX - owner.PrevX;
        var d1 = owner.PosZ - owner.PrevZ;

        if (d0 * d0 + d1 * d1 > 2.500000277905201E-7D)
        {
            owner.RotYBody = owner.RotY;
            owner.RotYHead = ComputeAngleWithBound(owner.RotYBody, owner.RotYHead, 75.0F);
            _prevRotYHead = owner.RotYHead;
            _rotationTickCounter = 0;
        }
        else
        {
            var f = 75.0F;

            if (Math.Abs(owner.RotYHead - _prevRotYHead) > 15.0F)
            {
                _rotationTickCounter = 0;
                _prevRotYHead = owner.RotYHead;
            }
            else
            {
                ++_rotationTickCounter;
                var i = 10;

                if (_rotationTickCounter > 10) f = Math.Max(1.0F - (_rotationTickCounter - 10) / 10.0F, 0.0F) * 75.0F;
            }

            owner.RotYBody = ComputeAngleWithBound(owner.RotYHead, owner.RotYBody, f);
        }
    }

    private float ComputeAngleWithBound(float p_75665_1_, float p_75665_2_, float p_75665_3_)
    {
        var f = Mth.WrapAngle(p_75665_1_ - p_75665_2_);
        if (f < -p_75665_3_) f = -p_75665_3_;
        if (f >= p_75665_3_) f = p_75665_3_;
        return p_75665_1_ - f;
    }
}