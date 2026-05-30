using System;
using System.Collections.Generic;
using Godot;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Render.Model;

public class ModelRenderer
{
    private const int DefaultTexWidth = 64;
    private const int DefaultTexHeight = 32;

    private ShaderMaterial _sharedMaterial;
    
    private static Shader _entityShader;

    public readonly EntityModel Model;
    private Node3D _root;

    private readonly Dictionary<string, MeshInstance3D> _nodes = new();

    public ModelRenderer(EntityModel model, string texturePath, Node3D parent,
        int texWidth = DefaultTexWidth, int texHeight = DefaultTexHeight)
    {
        Model = model;
        Build(texturePath, parent, texWidth, texHeight);
    }

    private void Build(string texturePath, Node3D parent, int texW, int texH)
    {
        _entityShader ??= GD.Load<Shader>("res://assets/shaders/entity.gdshader");

        var tex = GD.Load<Texture2D>(texturePath);
        if (tex == null) GD.PushError($"Failed to load texture: {texturePath}");

        _sharedMaterial = new ShaderMaterial { Shader = _entityShader };
        _sharedMaterial.SetShaderParameter("albedo_tex", tex);
        _sharedMaterial.SetShaderParameter("entity_color", new Color(0f, 0f, 0f, 0f));

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

    public void Update(float limbSwing, float limbSwingAmount, float ageInTicks, float netHeadYaw,
        float headPitch, float partialTicks, Entity entityIn)
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
        
        Color tint;
        if (entityIn.HurtTime > 0 || entityIn.DeathTime > 0) tint = new Color(1f, 0f, 0f, 0.3f);
        else tint = new Color(0f, 0f, 0f, 0f);
        _sharedMaterial.SetShaderParameter("entity_color", tint);
    }

    public void Free()
    {
        _root?.QueueFree();
        _root = null;
        _nodes.Clear();
        _sharedMaterial?.Dispose();
        _sharedMaterial = null;
    }

    public Node3D Root => _root;
}
