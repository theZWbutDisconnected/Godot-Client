using System.Collections.Generic;
using Godot;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Render.Model;

public abstract class EntityModel
{
    protected readonly List<ModelPart> Parts = new();

    public float SwingProgress;
    public bool IsRiding;

    public IReadOnlyList<ModelPart> GetParts() => Parts;

    public abstract void Animate(float limbSwing, float limbSwingAmount, float ageInTicks, float netHeadYaw, float headPitch, float scaleFactor, Entity entityIn);

    protected ModelPart AddPart(string name, float pivotX = 0f, float pivotY = 0f, float pivotZ = 0f)
    {
        var part = new ModelPart(name, new Vector3(pivotX, pivotY, pivotZ));
        Parts.Add(part);
        return part;
    }

    public static void CopyModelAngles(ModelPart source, ModelPart dest)
    {
        dest.XRot = source.XRot;
        dest.YRot = source.YRot;
        dest.ZRot = source.ZRot;
        dest.PivotX = source.PivotX;
        dest.PivotY = source.PivotY;
        dest.PivotZ = source.PivotZ;
        dest.Scale = source.Scale;
    }
}
