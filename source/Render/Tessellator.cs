using System;
using System.Collections.Generic;
using Godot;

namespace TestClient.Source.Render;

public sealed class Tessellator
{
    private const int MaxVertices = 786432 / 3;

    private static StandardMaterial3D _solidMaterial;
    private static StandardMaterial3D _liquidMaterial;
    private static Shader _solidAnimShader;
    private static Shader _liquidAnimShader;
    private static readonly Dictionary<AnimMatKey, ShaderMaterial> _animMaterialCache = new();

    private VertexAttributes _attrs;
    private Color _color = Colors.White;
    private int _count;
    private Vector3 _normal = Vector3.Zero;
    private SurfaceTool _sfTool;
    private Vector2 _uv = Vector2.Zero;

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
        // if (!_attrs.HasFlag(VertexAttributes.Normal))
        //     CheckFormatChange(VertexAttributes.Normal);
        // _attrs |= VertexAttributes.Normal;
        // _normal = new Vector3(x, y, z);
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
        _sfTool.GenerateNormals();
        Mesh mesh = _sfTool.Commit();

        Initialize();
        return mesh;
    }

    private MeshInstance3D BuildInstance(Mesh mesh, Material material)
    {
        if (mesh == null)
            return null;

        return new MeshInstance3D
        {
            Mesh = mesh,
            MaterialOverride = material ?? GetSolidMaterial()
        };
    }

    public MeshInstance3D BuildMeshInstance(Material material = null)
    {
        var mesh = Flush();
        return mesh != null ? BuildInstance(mesh, material) : null;
    }

    public static StandardMaterial3D GetSolidMaterial()
    {
        return _solidMaterial ??= CreateSolidMaterial();
    }

    public static StandardMaterial3D GetLiquidMaterial()
    {
        return _liquidMaterial ??= CreateLiquidMaterial();
    }

    public static ShaderMaterial GetOrCreateAnimMaterial(int texIndex, TextureAtlas.AnimData data, bool isLiquid)
    {
        var key = new AnimMatKey(texIndex, data);
        if (!_animMaterialCache.TryGetValue(key, out var mat))
        {
            mat = CreateAnimMaterial(texIndex, data, isLiquid);
            _animMaterialCache[key] = mat;
        }
        return mat;
    }

    private static ShaderMaterial CreateAnimMaterial(int texIndex, TextureAtlas.AnimData data, bool isLiquid)
    {
        var shader = isLiquid ? GetLiquidAnimShader() : GetSolidAnimShader();

        var mat = new ShaderMaterial { Shader = shader };
        mat.SetShaderParameter("albedo_tex", TextureAtlas.AtlasTexture);
        mat.SetShaderParameter("atlas_width", (float)TextureAtlas.AtlasWidth);
        mat.SetShaderParameter("first_frame_index", texIndex);
        mat.SetShaderParameter("frame_count", data.FrameCount);
        mat.SetShaderParameter("frame_time", data.FrameTime);
        mat.SetShaderParameter("pingpong", data.PingPong);
        mat.SetShaderParameter("frame_offset", data.FrameOffset);

        return mat;
    }

    private static Shader GetSolidAnimShader()
    {
        return _solidAnimShader ??= GD.Load<Shader>("res://assets/shaders/soild_animated.gdshader");
    }

    private static Shader GetLiquidAnimShader()
    {
        return _liquidAnimShader ??= GD.Load<Shader>("res://assets/shaders/liquid_animated.gdshader");
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

    public static StandardMaterial3D CreateSolidMaterial()
    {
        return new StandardMaterial3D
        {
            Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor,
            DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Always,
            AlbedoTexture = TextureAtlas.AtlasTexture,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            CullMode = BaseMaterial3D.CullModeEnum.Back,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel,
            Roughness = 1.0f,
            Metallic = 0.0f,
            VertexColorUseAsAlbedo = true
        };
    }

    public static StandardMaterial3D CreateLiquidMaterial()
    {
        return new StandardMaterial3D
        {
            Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass,
            DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Always,
            AlbedoTexture = TextureAtlas.AtlasTexture,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel,
            Roughness = 1.0f,
            Metallic = 0.0f,
            VertexColorUseAsAlbedo = true
        };
    }

    private static StandardMaterial3D CreateDefaultMaterial()
    {
        return CreateSolidMaterial();
    }

    // === Animation material cache key ===
    private struct AnimMatKey : IEquatable<AnimMatKey>
    {
        private readonly int TexIndex;
        private readonly int FrameCount;
        private readonly float FrameTime;
        private readonly bool PingPong;
        private readonly int FrameOffset;

        public AnimMatKey(int texIndex, TextureAtlas.AnimData data)
        {
            TexIndex = texIndex;
            FrameCount = data.FrameCount;
            FrameTime = data.FrameTime;
            PingPong = data.PingPong;
            FrameOffset = data.FrameOffset;
        }

        public bool Equals(AnimMatKey other)
        {
            return TexIndex == other.TexIndex
                && FrameCount == other.FrameCount
                && Math.Abs(FrameTime - other.FrameTime) < 0.001f
                && PingPong == other.PingPong
                && FrameOffset == other.FrameOffset;
        }

        public override bool Equals(object obj) => obj is AnimMatKey other && Equals(other);

        public override int GetHashCode()
        {
            return HashCode.Combine(TexIndex, FrameCount, FrameTime.GetHashCode(), PingPong, FrameOffset);
        }
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
