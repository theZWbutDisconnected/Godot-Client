using System;
using Godot;

namespace TestClient.Source.Render;

public sealed class Tessellator
{
    private static readonly Texture2D Terrain;
    private const int MaxVertices = 524288 / 3;
    private VertexAttributes _attrs;

    private Color _color = Colors.White;
    private int _count;
    private Vector3 _normal = Vector3.Zero;

    private SurfaceTool _sfTool;
    private Vector2 _uv = Vector2.Zero;

    static Tessellator()
    {
        Terrain = GD.Load<Texture2D>("res://terrain.png");
    }

    public Tessellator()
    {
        Initialize();
    }

    public void Initialize()
    {
        _sfTool?.Clear();
        _sfTool = new SurfaceTool();
        _sfTool.Begin(Mesh.PrimitiveType.Triangles);
        _count = 0;
        _attrs = VertexAttributes.None;

        _color = Colors.White;
        _uv = Vector2.Zero;
        _normal = Vector3.Zero;
    }

    public void Tex(float u, float v)
    {
        if (!_attrs.HasFlag(VertexAttributes.Uv))
            CheckFormatChange(VertexAttributes.Uv);
        _attrs |= VertexAttributes.Uv;
        _uv = new Vector2(u, v);
    }

    public void Color(float r, float g, float b)
    {
        if (!_attrs.HasFlag(VertexAttributes.Color))
            CheckFormatChange(VertexAttributes.Color);
        _attrs |= VertexAttributes.Color;
        _color = new Color(r, g, b);
    }

    public void Normal(float x, float y, float z)
    {
        if (!_attrs.HasFlag(VertexAttributes.Normal))
            CheckFormatChange(VertexAttributes.Normal);
        _attrs |= VertexAttributes.Normal;
        _normal = new Vector3(x, y, z);
    }

    public void Vertex(float x, float y, float z)
    {
        _sfTool.SetNormal(_normal);
        if (_attrs.HasFlag(VertexAttributes.Uv))
            _sfTool.SetUV(_uv);
        if (_attrs.HasFlag(VertexAttributes.Color))
            _sfTool.SetColor(_color);
        _sfTool.AddVertex(new Vector3(x, y, z));

        _count++;
        if (_count >= MaxVertices)
            Flush();
    }

    public void VertexUV(float x, float y, float z, float u, float v)
    {
        Tex(u, v);
        Vertex(x, y, z);
    }

    public void Color(int rgb)
    {
        var r = ((rgb >> 16) & 0xFF) / 255f;
        var g = ((rgb >> 8) & 0xFF) / 255f;
        var b = (rgb & 0xFF) / 255f;
        Color(r, g, b);
    }

    public void NoColor()
    {
        if (_attrs.HasFlag(VertexAttributes.Color))
            CheckFormatChange(VertexAttributes.None, VertexAttributes.Color);
        _attrs &= ~VertexAttributes.Color;
        _color = Colors.White;
    }

    public Mesh Flush()
    {
        if (_count == 0)
            return null;

        _sfTool.Index();
        Mesh mesh = _sfTool.Commit();

        Initialize();
        return mesh;
    }

    private MeshInstance3D BuildInstance(Mesh mesh, Node parent = null, BaseMaterial3D material = null)
    {
        if (mesh == null)
            return null;

        var meshInstance = new MeshInstance3D
        {
            Mesh = mesh,
            MaterialOverride = material ?? CreateDefaultMaterial()
        };

        if (parent != null)
            parent.AddChild(meshInstance);

        return meshInstance;
    }

    public MeshInstance3D BuildMeshInstance(Node parent = null, BaseMaterial3D material = null)
    {
        var mesh = Flush();
        return mesh != null ? BuildInstance(mesh, parent, material) : null;
    }

    private void CheckFormatChange(VertexAttributes addFlag = VertexAttributes.None,
        VertexAttributes removed = VertexAttributes.None)
    {
        if (_count == 0)
            return;

        Flush();
        _sfTool.Begin(Mesh.PrimitiveType.Triangles);
        var keep = _attrs & ~removed;
        if (keep.HasFlag(VertexAttributes.Normal))
            _sfTool.SetNormal(_normal);
        if (keep.HasFlag(VertexAttributes.Uv))
            _sfTool.SetUV(_uv);
        if (keep.HasFlag(VertexAttributes.Color))
            _sfTool.SetColor(_color);
    }

    private static StandardMaterial3D CreateDefaultMaterial()
    {
        var material = new StandardMaterial3D
        {
            Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor,
            DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Always,
            AlbedoTexture = Terrain,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            CullMode = BaseMaterial3D.CullModeEnum.Back,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            VertexColorUseAsAlbedo = true
        };
        return material;
    }

    [Flags]
    private enum VertexAttributes
    {
        None = 0,
        Color = 1 << 0,
        Uv = 1 << 1,
        Normal = 1 << 2
    }
}