using System;
using Godot;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Render.Model.impl;

public class ZombieModel : BipedModel
{
    public override void Animate(float limbSwing, float limbSwingAmount, float ageInTicks, float netHeadYaw,
        float headPitch, float scaleFactor, Entity entityIn)
    {
        if (entityIn.IsChild())
        {
            Head.Scale = 1.5f;
        }
        base.Animate(limbSwing, limbSwingAmount, ageInTicks, netHeadYaw, headPitch, scaleFactor, entityIn);
        float f = MathF.Sin(SwingProgress * (float)Math.PI);
        float f1 = MathF.Sin((1.0F - (1.0F - SwingProgress) * (1.0F - SwingProgress)) * (float)Math.PI);
        Arm1.ZRot = 0.0F;
        Arm0.ZRot = 0.0F;
        Arm1.YRot = -(0.1F - f * 0.6F);
        Arm0.YRot = 0.1F - f * 0.6F;
        Arm1.XRot = -MathF.PI / 2F;
        Arm0.XRot = -MathF.PI / 2F;
        Arm1.XRot -= f * 1.2F - f1 * 0.4F;
        Arm0.XRot -= f * 1.2F - f1 * 0.4F;
        Arm1.ZRot += MathF.Cos(ageInTicks * 0.09F) * 0.05F + 0.05F;
        Arm0.ZRot -= MathF.Cos(ageInTicks * 0.09F) * 0.05F + 0.05F;
        Arm1.XRot += MathF.Sin(ageInTicks * 0.067F) * 0.05F;
        Arm0.XRot -= MathF.Sin(ageInTicks * 0.067F) * 0.05F;
    }
}