using System;
using Godot;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Render.Model.impl;

public class FirstPersonArm
{
    private const float ModelScale = 0.0625F;
    private const int TexWidth = 64;
    private const int TexHeight = 64;

    private readonly Node3D _bobNode;
    private readonly Node3D _armRoot;
    private readonly ModelPart _rArm;

    private readonly float _defaultPivotX;
    private readonly float _defaultPivotY;
    private readonly float _defaultPivotZ;

    private readonly MeshInstance3D _armMesh;

    public float EquipProgress;

    public FirstPersonArm(Camera3D camera, string texturePath)
    {
        _bobNode = new Node3D { Name = "FirstPerson_Bob" };
        camera.AddChild(_bobNode);

        _armRoot = new Node3D { Name = "FirstPerson_Arm" };
        _armRoot.Scale = new Vector3(ModelScale, ModelScale, ModelScale);
        _armRoot.RotationDegrees = new Vector3(32.0f, 96.0f, -45.0f);
        _armRoot.Position = new Vector3(0.4f, -0.76f, -0.6f);
        _bobNode.AddChild(_armRoot);

        _rArm = new ModelPart("Arm0", new Vector3(-5f, 2f, 0f));
        _defaultPivotX = _rArm.PivotX;
        _defaultPivotY = _rArm.PivotY;
        _defaultPivotZ = _rArm.PivotZ;

        _rArm.AddBox(-3f, -2f, -2f, 4, 12, 4, 40, 16);

        var tex = GD.Load<Texture2D>(texturePath);
        var material = new StandardMaterial3D
        {
            AlbedoTexture = tex,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor,
            DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Always
        };

        var mesh = _rArm.BuildMesh(TexWidth, TexHeight);
        _armMesh = new MeshInstance3D
        {
            Name = "ArmMesh",
            Mesh = mesh,
            MaterialOverride = material
        };
        _armRoot.AddChild(_armMesh);
    }

    public void Update(Entity player, float partialTicks)
    {
        var swingProgress = player.GetSwingProgress(partialTicks);

        var bobX = -0.3f * Mathf.Sin(Mathf.Sqrt(swingProgress) * Mathf.Pi);
        var bobY =  0.4f * Mathf.Sin(Mathf.Sqrt(swingProgress) * Mathf.Pi * 2f);
        var bobZ = -0.4f * Mathf.Sin(swingProgress * Mathf.Pi);

        bobY += EquipProgress * -0.6f;

        _bobNode.Position = new Vector3(bobX, bobY, bobZ);

        var f3 = Mathf.Sin(swingProgress * swingProgress * Mathf.Pi);
        var f4 = Mathf.Sin(Mathf.Sqrt(swingProgress) * Mathf.Pi);

        _armRoot.RotationDegrees = new Vector3(32f, 96f + f4 * 70f, -45f + f3 * -20f);
    }

    public void Free()
    {
        _bobNode?.QueueFree();
    }
}
