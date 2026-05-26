using System;
using Godot;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Render.Model.impl;

public class ZombieModel : BipedModel
{
    public override void Animate(float limbSwing, float limbSwingAmount, float ageInTicks, float netHeadYaw,
        float headPitch, float scaleFactor, Entity entityIn)
    {
        base.Animate(limbSwing, limbSwingAmount, ageInTicks, netHeadYaw, headPitch, scaleFactor, entityIn);
        float f = MathF.Sin(SwingProgress * (float)Math.PI);
        float f1 = MathF.Sin((1.0F - (1.0F - SwingProgress) * (1.0F - SwingProgress)) * (float)Math.PI);
        RArm.ZRot = 0.0F;
        LArm.ZRot = 0.0F;
        RArm.YRot = -(0.1F - f * 0.6F);
        LArm.YRot = 0.1F - f * 0.6F;
        RArm.XRot = -MathF.PI / 2F;
        LArm.XRot = -MathF.PI / 2F;
        RArm.XRot -= f * 1.2F - f1 * 0.4F;
        LArm.XRot -= f * 1.2F - f1 * 0.4F;
        RArm.ZRot += MathF.Cos(ageInTicks * 0.09F) * 0.05F + 0.05F;
        LArm.ZRot -= MathF.Cos(ageInTicks * 0.09F) * 0.05F + 0.05F;
        RArm.XRot += MathF.Sin(ageInTicks * 0.067F) * 0.05F;
        LArm.XRot -= MathF.Sin(ageInTicks * 0.067F) * 0.05F;
    }
}