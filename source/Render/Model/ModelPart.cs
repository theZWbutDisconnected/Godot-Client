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
    public float Scale = 1f;
    public bool Mirror;

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
        _boxes.Add(new ModelBox(x0, y0, z0, w, h, d, texU, texV, Mirror));
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

        float ruL, ruR, rvT, rvB;
        float luL, luR, lvT, lvB;
        float duL, duR, dvT, dvB;
        float uuL, uuR, uvT, uvB;

        if (box.Mirror)
        {
            ruL = (u + d + w) * iw;        ruR = (u + d + w + d) * iw;
            rvB = (v + d) * ih;            rvT = (v + d + h) * ih;
            luL = u * iw;                  luR = (u + d) * iw;
            lvB = (v + d) * ih;            lvT = (v + d + h) * ih;
            duL = (u + d) * iw;            duR = (u + d + w) * iw;
            dvB = v * ih;                  dvT = (v + d) * ih;
            uuL = (u + d + w + w) * iw;    uuR = (u + d + w) * iw;
            uvB = v * ih;                  uvT = (v + d) * ih;
        }
        else
        {
            ruL = (u + d) * iw;            ruR = u * iw;
            rvB = (v + d) * ih;            rvT = (v + d + h) * ih;
            luL = (u + d + w + d) * iw;    luR = (u + d + w) * iw;
            lvB = (v + d) * ih;            lvT = (v + d + h) * ih;
            duL = (u + d + w) * iw;        duR = (u + d) * iw;
            dvB = v * ih;                  dvT = (v + d) * ih;
            uuL = (u + d + w) * iw;        uuR = (u + d + w + w) * iw;
            uvB = v * ih;                  uvT = (v + d) * ih;
        }

        AddQuad(st,
            new Vector3(x1, y0, z0), new Vector3(x1, y1, z0),
            new Vector3(x1, y1, z1), new Vector3(x1, y0, z1),
            Vector3.Right,
            ruL, rvB, ruL, rvT, ruR, rvT, ruR, rvB);

        AddQuad(st,
            new Vector3(x0, y0, z1), new Vector3(x0, y1, z1),
            new Vector3(x0, y1, z0), new Vector3(x0, y0, z0),
            Vector3.Left,
            luL, lvB, luL, lvT, luR, lvT, luR, lvB);

        AddQuad(st,
            new Vector3(x0, y0, z1), new Vector3(x0, y0, z0),
            new Vector3(x1, y0, z0), new Vector3(x1, y0, z1),
            Vector3.Down,
            duL, dvB, duL, dvT, duR, dvT, duR, dvB);

        AddQuad(st,
            new Vector3(x0, y1, z0), new Vector3(x0, y1, z1),
            new Vector3(x1, y1, z1), new Vector3(x1, y1, z0),
            Vector3.Up,
            uuL, uvB, uuL, uvT, uuR, uvT, uuR, uvB);

        AddQuad(st,
            new Vector3(x0, y1, z0), new Vector3(x0, y0, z0),
            new Vector3(x1, y0, z0), new Vector3(x1, y1, z0),
            Vector3.Back,
            (u + d + w) * iw, (v + d + h) * ih,
            (u + d + w) * iw, (v + d) * ih,
            (u + d) * iw, (v + d) * ih,
            (u + d) * iw, (v + d + h) * ih);

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