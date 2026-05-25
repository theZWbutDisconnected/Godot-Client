using System;
using System.Collections.Generic;
using Godot;

namespace TestClient.Source.Render.Model;

public class ModelRenderer
{
    private const int DefaultTexWidth = 64;
    private const int DefaultTexHeight = 32;

    private static StandardMaterial3D s_sharedMaterial;
    private static string s_loadedTexturePath;

    private readonly EntityModel _model;
    private Node3D _root;

    private readonly Dictionary<string, MeshInstance3D> _nodes = new();

    public ModelRenderer(EntityModel model, string texturePath, Node3D parent,
        int texWidth = DefaultTexWidth, int texHeight = DefaultTexHeight)
    {
        _model = model;
        Build(texturePath, parent, texWidth, texHeight);
    }

    private void Build(string texturePath, Node3D parent, int texW, int texH)
    {
        EnsureMaterial(texturePath);

        _root = new Node3D { Name = $"{_model.GetType().Name}_Root" };
        parent.AddChild(_root);

        foreach (var part in _model.GetParts())
        {
            var mesh = part.BuildMesh(texW, texH);
            var node = new MeshInstance3D
            {
                Name = part.Name,
                Mesh = mesh,
                MaterialOverride = s_sharedMaterial
            };
            _root.AddChild(node);
            _nodes[part.Name] = node;
        }
    }

    public void Update(double time)
    {
        _model.Animate(time);

        foreach (var part in _model.GetParts())
        {
            if (!_nodes.TryGetValue(part.Name, out var node))
                continue;

            node.Position = new Vector3(part.PivotX, part.PivotY, part.PivotZ);
            node.Rotation = Vector3.Zero;
            node.RotateX(part.XRot);
            node.RotateY(part.YRot);
            node.RotateZ(part.ZRot);
        }
    }

    public void Free()
    {
        _root?.QueueFree();
        _root = null;
        _nodes.Clear();
    }

    public Node3D Root => _root;

    private static void EnsureMaterial(string texturePath)
    {
        if (s_sharedMaterial != null && s_loadedTexturePath == texturePath)
            return;

        s_sharedMaterial?.Dispose();
        s_sharedMaterial = null;

        var tex = GD.Load<Texture2D>(texturePath);
        if (tex == null)
        {
            GD.PushError($"Failed to load texture: {texturePath}");
            s_sharedMaterial = new StandardMaterial3D
            {
                AlbedoColor = new Color(1f, 0f, 1f),
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
            };
            return;
        }

        s_sharedMaterial = new StandardMaterial3D
        {
            AlbedoTexture = tex,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor,
            AlphaScissorThreshold = 0.1f
        };

        s_loadedTexturePath = texturePath;
    }
}
