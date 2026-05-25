using System.Collections.Generic;
using Godot;

namespace TestClient.Source.Render.Model;

public class ModelPart
{
    public readonly string Name;
    public float PivotX;
    public float PivotY;
    public float PivotZ;
    public float XRot;
    public float YRot;
    public float ZRot;

    private readonly List<ModelBox> _boxes = new();
    private ArrayMesh _mesh;
    private int _builtTexW;
    private int _builtTexH;

    public ModelPart(string name)
    {
        Name = name;
    }

    public ModelPart(string name, Vector3 pivot) : this(name)
    {
        PivotX = pivot.X;
        PivotY = pivot.Y;
        PivotZ = pivot.Z;
    }

    public void AddBox(float x0, float y0, float z0, int w, int h, int d, int texU, int texV)
    {
        _boxes.Add(new ModelBox(x0, y0, z0, w, h, d, texU, texV));
    }

    public ArrayMesh BuildMesh(int texWidth, int texHeight)
    {
        if (_mesh != null && _builtTexW == texWidth && _builtTexH == texHeight)
            return _mesh;

        _builtTexW = texWidth;
        _builtTexH = texHeight;

        var st = new SurfaceTool();
        st.Begin(Mesh.PrimitiveType.Triangles);

        foreach (var box in _boxes)
            BuildBox(st, box, texWidth, texHeight);

        st.Index();
        _mesh = (ArrayMesh)st.Commit();
        return _mesh;
    }

    public void ClearMesh()
    {
        _mesh?.Dispose();
        _mesh = null;
    }

    private static void BuildBox(SurfaceTool st, ModelBox box, int texW, int texH)
    {
        float x0 = box.X0, x1 = box.X0 + box.W;
        float y0 = box.Y0, y1 = box.Y0 + box.H;
        float z0 = box.Z0, z1 = box.Z0 + box.D;

        int u = box.TexU, v = box.TexV;
        int w = box.W, h = box.H, d = box.D;

        float iw = 1f / texW;
        float ih = 1f / texH;

        AddQuad(st,
            new Vector3(x1, y0, z0), new Vector3(x1, y1, z0),
            new Vector3(x1, y1, z1), new Vector3(x1, y0, z1),
            Vector3.Right,
            (u + d + w) * iw, (v + d) * ih,
            (u + d + w) * iw, (v + d + h) * ih,
            (u + d + w + d) * iw, (v + d + h) * ih,
            (u + d + w + d) * iw, (v + d) * ih);

        AddQuad(st,
            new Vector3(x0, y0, z1), new Vector3(x0, y1, z1),
            new Vector3(x0, y1, z0), new Vector3(x0, y0, z0),
            Vector3.Left,
            u * iw, (v + d) * ih,
            u * iw, (v + d + h) * ih,
            (u + d) * iw, (v + d + h) * ih,
            (u + d) * iw, (v + d) * ih);

        AddQuad(st,
            new Vector3(x0, y0, z1), new Vector3(x0, y0, z0),
            new Vector3(x1, y0, z0), new Vector3(x1, y0, z1),
            Vector3.Down,
            (u + d + w) * iw, v * ih,
            (u + d + w) * iw, (v + d) * ih,
            (u + d) * iw, (v + d) * ih,
            (u + d) * iw, v * ih);

        AddQuad(st,
            new Vector3(x0, y1, z0), new Vector3(x0, y1, z1),
            new Vector3(x1, y1, z1), new Vector3(x1, y1, z0),
            Vector3.Up,
            (u + d + w) * iw, v * ih,
            (u + d + w) * iw, (v + d) * ih,
            (u + d + w + w) * iw, (v + d) * ih,
            (u + d + w + w) * iw, v * ih);

        AddQuad(st,
            new Vector3(x0, y1, z0), new Vector3(x0, y0, z0),
            new Vector3(x1, y0, z0), new Vector3(x1, y1, z0),
            Vector3.Back,
            (u + d) * iw, (v + d + h) * ih,
            (u + d) * iw, (v + d) * ih,
            (u + d + w) * iw, (v + d) * ih,
            (u + d + w) * iw, (v + d + h) * ih);

        AddQuad(st,
            new Vector3(x1, y1, z1), new Vector3(x1, y0, z1),
            new Vector3(x0, y0, z1), new Vector3(x0, y1, z1),
            Vector3.Forward,
            (u + d + w + d + w) * iw, (v + d + h) * ih,
            (u + d + w + d + w) * iw, (v + d) * ih,
            (u + d + w + d) * iw, (v + d) * ih,
            (u + d + w + d) * iw, (v + d + h) * ih);
    }

    private static void AddQuad(SurfaceTool st,
        Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3,
        Vector3 normal,
        float u0, float v0, float u1, float v1,
        float u2, float v2, float u3, float v3)
    {
        st.SetNormal(normal);

        st.SetUV(new Vector2(u0, v0));
        st.AddVertex(p0);
        st.SetUV(new Vector2(u1, v1));
        st.AddVertex(p1);
        st.SetUV(new Vector2(u2, v2));
        st.AddVertex(p2);

        st.SetUV(new Vector2(u0, v0));
        st.AddVertex(p0);
        st.SetUV(new Vector2(u2, v2));
        st.AddVertex(p2);
        st.SetUV(new Vector2(u3, v3));
        st.AddVertex(p3);
    }
}
