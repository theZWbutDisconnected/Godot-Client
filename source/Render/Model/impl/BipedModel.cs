using System;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Render.Model.impl;

public class BipedModel : EntityModel
{
    public readonly ModelPart Arm0;
    public readonly ModelPart Arm1;
    public readonly ModelPart Body;
    public readonly ModelPart Head;
    public readonly ModelPart Leg0;
    public readonly ModelPart Leg1;
    public readonly ModelPart Headwear;
    public int HeldItemLeft;
    public int HeldItemRight;
    public bool IsSneak;
    public bool AimedBow;

    public BipedModel()
    {
        Head = AddPart("Head");
        Head.AddBox(-4f, -8f, -4f, 8, 8, 8, 0, 0);

        Headwear = AddPart("Headwear");
        Headwear.AddBox(-4f, -8f, -4f, 8, 8, 8, 32, 0);

        Body = AddPart("Body");
        Body.AddBox(-4f, 0f, -2f, 8, 12, 4, 16, 16);

        Arm0 = AddPart("Arm0", -5f, 2f);
        Arm0.AddBox(-3f, -2f, -2f, 4, 12, 4, 40, 16);

        Arm1 = AddPart("Arm1", 5f, 2f);
        Arm1.Mirror = true;
        Arm1.AddBox(-1f, -2f, -2f, 4, 12, 4, 40, 16);

        Leg0 = AddPart("Leg0", -2f, 12f);
        Leg0.AddBox(-2f, 0f, -2f, 4, 12, 4, 0, 16);

        Leg1 = AddPart("Leg1", 2f, 12f);
        Leg1.Mirror = true;
        Leg1.AddBox(-2f, 0f, -2f, 4, 12, 4, 0, 16);
    }

    public override void Animate(float limbSwing, float limbSwingAmount, float ageInTicks, float netHeadYaw, float headPitch, float scaleFactor, Entity entityIn)
    {
        Head.YRot = netHeadYaw / (180F / MathF.PI);
        Head.XRot = headPitch / (180F / MathF.PI);

        Arm1.XRot = MathF.Cos(limbSwing * 0.6662F + MathF.PI) * 2.0F * limbSwingAmount * 0.5F;
        Arm0.XRot = MathF.Cos(limbSwing * 0.6662F) * 2.0F * limbSwingAmount * 0.5F;
        Arm1.ZRot = 0.0F;
        Arm0.ZRot = 0.0F;
        Leg1.XRot = MathF.Cos(limbSwing * 0.6662F) * 1.4F * limbSwingAmount;
        Leg0.XRot = MathF.Cos(limbSwing * 0.6662F + MathF.PI) * 1.4F * limbSwingAmount;
        Leg1.YRot = 0.0F;
        Leg0.YRot = 0.0F;

        if (IsRiding)
        {
            Arm1.XRot += -MathF.PI / 5F;
            Arm0.XRot += -MathF.PI / 5F;
            Leg1.XRot = -MathF.PI * 2F / 5F;
            Leg0.XRot = -MathF.PI * 2F / 5F;
            Leg1.YRot = MathF.PI / 10F;
            Leg0.YRot = -MathF.PI / 10F;
        }

        if (HeldItemLeft != 0)
        {
            Arm0.XRot = Arm0.XRot * 0.5F - (MathF.PI / 10F) * HeldItemLeft;
        }

        Arm1.YRot = 0.0F;
        Arm1.ZRot = 0.0F;

        switch (HeldItemRight)
        {
            case 1:
                Arm1.XRot = Arm1.XRot * 0.5F - (MathF.PI / 10F) * HeldItemRight;
                break;
            case 3:
                Arm1.XRot = Arm1.XRot * 0.5F - (MathF.PI / 10F) * HeldItemRight;
                Arm1.YRot = -0.5235988F;
                break;
        }

        Arm0.YRot = 0.0F;

        if (SwingProgress > -9990.0F)
        {
            float f = SwingProgress;
            Body.YRot = MathF.Sin(MathF.Sqrt(f) * MathF.PI * 2.0F) * 0.2F;
            Arm1.PivotZ = MathF.Sin(Body.YRot) * 5.0F;
            Arm1.PivotX = -MathF.Cos(Body.YRot) * 5.0F;
            Arm0.PivotZ = -MathF.Sin(Body.YRot) * 5.0F;
            Arm0.PivotX = MathF.Cos(Body.YRot) * 5.0F;
            Arm0.PivotX += 2.0f;
            Arm1.PivotX -= 2.0f;
            Arm1.YRot += Body.YRot;
            Arm0.YRot += Body.YRot;
            Arm0.XRot += Body.YRot;
            f = 1.0F - SwingProgress;
            f = f * f;
            f = f * f;
            f = 1.0F - f;
            float f1 = MathF.Sin(f * MathF.PI);
            float f2 = MathF.Sin(SwingProgress * MathF.PI) * -(Head.XRot - 0.7F) * 0.75F;
            Arm1.XRot -= f1 * 1.2F + f2;
            Arm1.YRot += Body.YRot * 2.0F;
            Arm1.ZRot += MathF.Sin(SwingProgress * MathF.PI) * -0.4F;
        }

        if (IsSneak)
        {
            Body.XRot = 0.5F;
            Arm1.XRot += 0.4F;
            Arm0.XRot += 0.4F;
            Leg1.PivotZ = 4.0F;
            Leg0.PivotZ = 4.0F;
            Leg1.PivotY = 9.0F;
            Leg0.PivotY = 9.0F;
            Head.PivotY = 1.0F;
        }
        else
        {
            Body.XRot = 0.0F;
            Leg1.PivotZ = 0.1F;
            Leg0.PivotZ = 0.1F;
            Leg1.PivotY = 12.0F;
            Leg0.PivotY = 12.0F;
            Head.PivotY = 0.0F;
        }

        Arm1.ZRot += MathF.Cos(ageInTicks * 0.09F) * 0.05F + 0.05F;
        Arm0.ZRot -= MathF.Cos(ageInTicks * 0.09F) * 0.05F + 0.05F;
        Arm1.XRot += MathF.Sin(ageInTicks * 0.067F) * 0.05F;
        Arm0.XRot -= MathF.Sin(ageInTicks * 0.067F) * 0.05F;

        if (AimedBow)
        {
            float f3 = 0.0F;
            float f4 = 0.0F;
            Arm1.ZRot = 0.0F;
            Arm0.ZRot = 0.0F;
            Arm1.YRot = -(0.1F - f3 * 0.6F) + Head.YRot;
            Arm0.YRot = 0.1F - f3 * 0.6F + Head.YRot + 0.4F;
            Arm1.XRot = -MathF.PI / 2F + Head.XRot;
            Arm0.XRot = -MathF.PI / 2F + Head.XRot;
            Arm1.XRot -= f3 * 1.2F - f4 * 0.4F;
            Arm0.XRot -= f3 * 1.2F - f4 * 0.4F;
            Arm1.ZRot += MathF.Cos(ageInTicks * 0.09F) * 0.05F + 0.05F;
            Arm0.ZRot -= MathF.Cos(ageInTicks * 0.09F) * 0.05F + 0.05F;
            Arm1.XRot += MathF.Sin(ageInTicks * 0.067F) * 0.05F;
            Arm0.XRot -= MathF.Sin(ageInTicks * 0.067F) * 0.05F;
        }

        if (entityIn.IsChild())
        {
            Head.Scale = 1.5f;
        }
        
        CopyModelAngles(Head, Headwear);
    }
}