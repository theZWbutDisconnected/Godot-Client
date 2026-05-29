using System;
using System.Collections.Generic;
using Godot;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Render.Model;

public class ModelRenderer
{
    private const int DefaultTexWidth = 64;
    private const int DefaultTexHeight = 32;

    private Texture2D _tex;
    private StandardMaterial3D _sharedMaterial;
    private StandardMaterial3D _hurtOverlayMaterial;

    public readonly EntityModel Model;
    private Node3D _root;
    private float _lastHurtAlpha = -1f;

    private readonly Dictionary<string, MeshInstance3D> _nodes = new();

    public ModelRenderer(EntityModel model, string texturePath, Node3D parent,
        int texWidth = DefaultTexWidth, int texHeight = DefaultTexHeight)
    {
        Model = model;
        Build(texturePath, parent, texWidth, texHeight);
    }

    private void Build(string texturePath, Node3D parent, int texW, int texH)
    {
        _sharedMaterial ??= new StandardMaterial3D
        {
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor
        };
        _hurtOverlayMaterial ??= new StandardMaterial3D
        {
            AlbedoColor = new Color(1f, 0f, 0f, 0f),
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            RenderPriority = 1,
            DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Disabled,
            NoDepthTest = false
        };
        
        _tex = GD.Load<Texture2D>(texturePath);
        if (_tex == null)
        {
            GD.PushError($"Failed to load texture: {texturePath}");
            _sharedMaterial.AlbedoColor = new Color(1f, 0f, 1f, 1f);
        } else
        {
            _sharedMaterial.AlbedoTexture = _tex;
            _hurtOverlayMaterial.AlbedoTexture = _tex;
        }

        _root = new Node3D { Name = $"{Model.GetType().Name}_Root" };
        parent.AddChild(_root);

        foreach (var part in Model.GetParts())
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

    public void Update(float limbSwing, float limbSwingAmount, float ageInTicks, float netHeadYaw, float headPitch, float partialTicks, Entity entityIn)
    {
        Model.SwingProgress = entityIn.GetSwingProgress(partialTicks);
        Model.IsRiding = entityIn.IsRiding();
        Model.Animate(limbSwing, limbSwingAmount, ageInTicks, netHeadYaw, headPitch, partialTicks, entityIn);

        foreach (var part in Model.GetParts())
        {
            if (!_nodes.TryGetValue(part.Name, out var node))
                continue;

            node.Scale = new Vector3(part.Scale, part.Scale, part.Scale);
            node.Position = new Vector3(part.PivotX, part.PivotY, part.PivotZ);
            node.Rotation = Vector3.Zero;
            node.RotateX(part.XRot);
            node.RotateY(part.YRot);
            node.RotateZ(part.ZRot);
        }

        var a = entityIn.HurtTime;
        float smoothed = Math.Max(0f, a - partialTicks);
        float alpha = (smoothed / 10f) * 0.5f + 0.5f;
        if (a < 5) alpha = 0;
        SetHurtAlpha(alpha);
    }

    private void SetHurtAlpha(float alpha)
    {
        float quantized = MathF.Round(alpha * 255f) / 255f;
        if (Math.Abs(quantized - _lastHurtAlpha) < 1e-4f)
            return;

        _lastHurtAlpha = quantized;

        if (quantized <= 0f)
        {
            SetNextPassOnAll(null);
        }
        else
        {
            _hurtOverlayMaterial.AlbedoColor = new Color(1f, 0f, 0f, quantized);
            SetNextPassOnAll(_hurtOverlayMaterial);
        }
    }

    private void SetNextPassOnAll(Material mat)
    {
        foreach (var node in _nodes.Values)
            node.MaterialOverlay = mat;
    }

    public void Free()
    {
        _root?.QueueFree();
        _root = null;
        _nodes.Clear();
        _sharedMaterial?.Dispose();
        _sharedMaterial = null;
        _hurtOverlayMaterial?.Dispose();
        _hurtOverlayMaterial = null;
    }

    public Node3D Root => _root;
}
