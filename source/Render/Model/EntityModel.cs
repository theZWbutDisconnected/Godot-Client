using System.Collections.Generic;
using Godot;

namespace TestClient.Source.Render.Model;

public abstract class EntityModel
{
    protected readonly List<ModelPart> Parts = new();

    public IReadOnlyList<ModelPart> GetParts() => Parts;

    public abstract void Animate(double time);

    protected ModelPart AddPart(string name, float pivotX = 0f, float pivotY = 0f, float pivotZ = 0f)
    {
        var part = new ModelPart(name, new Vector3(pivotX, pivotY, pivotZ));
        Parts.Add(part);
        return part;
    }
}
