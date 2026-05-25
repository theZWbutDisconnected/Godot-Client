using System;

namespace TestClient.Source.Render.Model.impl;

public class ZombieModel : EntityModel
{
    public readonly ModelPart Arm0;
    public readonly ModelPart Arm1;
    public readonly ModelPart Body;
    public readonly ModelPart Head;
    public readonly ModelPart Leg0;
    public readonly ModelPart Leg1;

    public ZombieModel()
    {
        Head = AddPart("Head");
        Head.AddBox(-4f, -8f, -4f, 8, 8, 8, 0, 0);

        Body = AddPart("Body");
        Body.AddBox(-4f, 0f, -2f, 8, 12, 4, 16, 16);

        Arm0 = AddPart("Arm0", -5f, 2f);
        Arm0.AddBox(-3f, -2f, -2f, 4, 12, 4, 40, 16);

        Arm1 = AddPart("Arm1", 5f, 2f);
        Arm1.AddBox(-1f, -2f, -2f, 4, 12, 4, 40, 16);

        Leg0 = AddPart("Leg0", -2f, 12f);
        Leg0.AddBox(-2f, 0f, -2f, 4, 12, 4, 0, 16);

        Leg1 = AddPart("Leg1", 2f, 12f);
        Leg1.AddBox(-2f, 0f, -2f, 4, 12, 4, 0, 16);
    }

    public override void Animate(double time)
    {
        Head.YRot = (float)Math.Sin(time * 0.83) * 1.0F;
        Head.XRot = (float)Math.Sin(time) * 0.8F;
        Arm0.XRot = (float)Math.Sin(time * 0.6662 + Math.PI) * 2.0F;
        Arm0.ZRot = (float)(Math.Sin(time * 0.2312) + 1.0F) * 1.0F;
        Arm1.XRot = (float)Math.Sin(time * 0.6662) * 2.0F;
        Arm1.ZRot = (float)(Math.Sin(time * 0.2812) - 1.0F) * 1.0F;
        Leg0.XRot = (float)Math.Sin(time * 0.6662) * 1.4F;
        Leg1.XRot = (float)Math.Sin(time * 0.6662 + Math.PI) * 1.4F;
    }
}