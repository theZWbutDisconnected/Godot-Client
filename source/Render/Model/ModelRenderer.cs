using System;
using System.Collections.Generic;
using Godot;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Render.Model;

public class ModelRenderer
{
    private const int DefaultTexWidth = 64;
    private const int DefaultTexHeight = 32;

    private StandardMaterial3D _sharedMaterial;
    private string _loadedTexturePath;

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
                MaterialOverride = _sharedMaterial
            };
            _root.AddChild(node);
            _nodes[part.Name] = node;
        }
    }

    public void Update(float limbSwing, float limbSwingAmount, float ageInTicks, float netHeadYaw, float headPitch, float scaleFactor, Entity entityIn)
    {
        _model.SwingProgress = entityIn.GetSwingProgress(Game.Singleton.Timer.RenderPartialTicks);
        _model.IsRiding = entityIn.IsRiding();
        _model.Animate(limbSwing, limbSwingAmount, ageInTicks, netHeadYaw, headPitch, scaleFactor, entityIn);

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

    private void EnsureMaterial(string texturePath)
    {
        if (_sharedMaterial != null && _loadedTexturePath == texturePath)
            return;

        _sharedMaterial?.Dispose();
        _sharedMaterial = null;

        var tex = GD.Load<Texture2D>(texturePath);
        if (tex == null)
        {
            GD.PushError($"Failed to load texture: {texturePath}");
            _sharedMaterial = new StandardMaterial3D
            {
                AlbedoColor = new Color(1f, 0f, 1f),
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded
            };
            return;
        }

        _sharedMaterial = new StandardMaterial3D
        {
            AlbedoTexture = tex,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor,
            AlphaScissorThreshold = 0.1f
        };

        _loadedTexturePath = texturePath;
    }
}
