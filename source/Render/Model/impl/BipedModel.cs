using System;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Render.Model.impl;

public class BipedModel : EntityModel
{
    public readonly ModelPart RArm;
    public readonly ModelPart LArm;
    public readonly ModelPart Body;
    public readonly ModelPart Head;
    public readonly ModelPart RLeg;
    public readonly ModelPart LLeg;
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

        RArm = AddPart("Arm0", -5f, 2f);
        RArm.AddBox(-3f, -2f, -2f, 4, 12, 4, 40, 16);

        LArm = AddPart("Arm1", 5f, 2f);
        LArm.Mirror = true;
        LArm.AddBox(-1f, -2f, -2f, 4, 12, 4, 40, 16);

        RLeg = AddPart("Leg0", -2f, 12f);
        RLeg.AddBox(-2f, 0f, -2f, 4, 12, 4, 0, 16);

        LLeg = AddPart("Leg1", 2f, 12f);
        LLeg.Mirror = true;
        LLeg.AddBox(-2f, 0f, -2f, 4, 12, 4, 0, 16);
    }

    public override void Animate(float limbSwing, float limbSwingAmount, float ageInTicks, float netHeadYaw, float headPitch, float scaleFactor, Entity entityIn)
    {
        Head.YRot = netHeadYaw / (180F / MathF.PI);
        Head.XRot = headPitch / (180F / MathF.PI);

        RArm.XRot = MathF.Cos(limbSwing * 0.6662F + MathF.PI) * 2.0F * limbSwingAmount * 0.5F;
        LArm.XRot = MathF.Cos(limbSwing * 0.6662F) * 2.0F * limbSwingAmount * 0.5F;
        RArm.ZRot = 0.0F;
        LArm.ZRot = 0.0F;
        RLeg.XRot = MathF.Cos(limbSwing * 0.6662F) * 1.4F * limbSwingAmount;
        LLeg.XRot = MathF.Cos(limbSwing * 0.6662F + MathF.PI) * 1.4F * limbSwingAmount;
        RLeg.YRot = 0.0F;
        LLeg.YRot = 0.0F;

        if (IsRiding)
        {
            RArm.XRot += -MathF.PI / 5F;
            LArm.XRot += -MathF.PI / 5F;
            RLeg.XRot = -MathF.PI * 2F / 5F;
            LLeg.XRot = -MathF.PI * 2F / 5F;
            RLeg.YRot = MathF.PI / 10F;
            LLeg.YRot = -MathF.PI / 10F;
        }

        if (HeldItemLeft != 0)
        {
            LArm.XRot = LArm.XRot * 0.5F - (MathF.PI / 10F) * HeldItemLeft;
        }

        RArm.YRot = 0.0F;
        RArm.ZRot = 0.0F;

        switch (HeldItemRight)
        {
            case 1:
                RArm.XRot = RArm.XRot * 0.5F - (MathF.PI / 10F) * HeldItemRight;
                break;
            case 3:
                RArm.XRot = RArm.XRot * 0.5F - (MathF.PI / 10F) * HeldItemRight;
                RArm.YRot = -0.5235988F;
                break;
        }

        LArm.YRot = 0.0F;

        if (SwingProgress > -9990.0F)
        {
            float f = SwingProgress;
            Body.YRot = MathF.Sin(MathF.Sqrt(f) * MathF.PI * 2.0F) * 0.2F;
            RArm.PivotZ = MathF.Sin(Body.YRot) * 5.0F;
            RArm.PivotX = -MathF.Cos(Body.YRot) * 5.0F;
            LArm.PivotZ = -MathF.Sin(Body.YRot) * 5.0F;
            LArm.PivotX = MathF.Cos(Body.YRot) * 5.0F;
            RArm.YRot += Body.YRot;
            LArm.YRot += Body.YRot;
            LArm.XRot += Body.YRot;
            f = 1.0F - SwingProgress;
            f = f * f;
            f = f * f;
            f = 1.0F - f;
            float f1 = MathF.Sin(f * MathF.PI);
            float f2 = MathF.Sin(SwingProgress * MathF.PI) * -(Head.XRot - 0.7F) * 0.75F;
            RArm.XRot -= f1 * 1.2F + f2;
            RArm.YRot += Body.YRot * 2.0F;
            RArm.ZRot += MathF.Sin(SwingProgress * MathF.PI) * -0.4F;
        }

        if (IsSneak)
        {
            Body.XRot = 0.5F;
            RArm.XRot += 0.4F;
            LArm.XRot += 0.4F;
            RLeg.PivotZ = 4.0F;
            LLeg.PivotZ = 4.0F;
            RLeg.PivotY = 9.0F;
            LLeg.PivotY = 9.0F;
            Head.PivotY = 1.0F;
        }
        else
        {
            Body.XRot = 0.0F;
            RLeg.PivotZ = 0.1F;
            LLeg.PivotZ = 0.1F;
            RLeg.PivotY = 12.0F;
            LLeg.PivotY = 12.0F;
            Head.PivotY = 0.0F;
        }

        RArm.ZRot += MathF.Cos(ageInTicks * 0.09F) * 0.05F + 0.05F;
        LArm.ZRot -= MathF.Cos(ageInTicks * 0.09F) * 0.05F + 0.05F;
        RArm.XRot += MathF.Sin(ageInTicks * 0.067F) * 0.05F;
        LArm.XRot -= MathF.Sin(ageInTicks * 0.067F) * 0.05F;

        if (AimedBow)
        {
            float f3 = 0.0F;
            float f4 = 0.0F;
            RArm.ZRot = 0.0F;
            LArm.ZRot = 0.0F;
            RArm.YRot = -(0.1F - f3 * 0.6F) + Head.YRot;
            LArm.YRot = 0.1F - f3 * 0.6F + Head.YRot + 0.4F;
            RArm.XRot = -MathF.PI / 2F + Head.XRot;
            LArm.XRot = -MathF.PI / 2F + Head.XRot;
            RArm.XRot -= f3 * 1.2F - f4 * 0.4F;
            LArm.XRot -= f3 * 1.2F - f4 * 0.4F;
            RArm.ZRot += MathF.Cos(ageInTicks * 0.09F) * 0.05F + 0.05F;
            LArm.ZRot -= MathF.Cos(ageInTicks * 0.09F) * 0.05F + 0.05F;
            RArm.XRot += MathF.Sin(ageInTicks * 0.067F) * 0.05F;
            LArm.XRot -= MathF.Sin(ageInTicks * 0.067F) * 0.05F;
        }

        if (entityIn.IsChild())
        {
            Head.Scale = 1.5f;
        }
        
        CopyModelAngles(Head, Headwear);
    }
}