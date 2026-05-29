using System;
using Godot;
using TestClient.Source.Physics;
using TestClient.Source.Render.Model;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Render;

public class FirstPersonArm
{
    private const float ModelScale = 0.0625F;
    private const int TexWidth = 64;
    private const int TexHeight = 64;

    private readonly Node3D _bobRoot;
    private readonly Node3D _armRoot;
    private readonly ModelPart _rArm;
    private readonly MeshInstance3D _armMesh;

    public FirstPersonArm(Game game, string texturePath)
    {
        _bobRoot = new Node3D { Name = "BobRoot" };
        game.AddChild(_bobRoot);
        
        _armRoot = new Node3D { Name = "ArmRoot" };
        _armRoot.Scale = new Vector3(ModelScale, ModelScale, ModelScale);
        _bobRoot.AddChild(_armRoot);

        _rArm = new ModelPart("Arm0", new Vector3(-5f, 2f, 0f));
        _rArm.AddBox(-3f, -2f, -2f, 4, 12, 4, 40, 16);

        var tex = GD.Load<Texture2D>(texturePath);
        var material = new StandardMaterial3D
        {
            AlbedoTexture = tex,
            TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor
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

    public void Update(Player player, float partialTicks)
    {
        var swingProgress = player.GetSwingProgress(partialTicks);

        var bobX = -0.3f * Mathf.Sin(Mathf.Sqrt(swingProgress) * Mathf.Pi);
        var bobY =  0.4f * Mathf.Sin(Mathf.Sqrt(swingProgress) * Mathf.Pi * 2f);
        var bobZ = -0.4f * Mathf.Sin(swingProgress * Mathf.Pi);
        bobY += player.EquipProgress * -0.6f;
        
        if (swingProgress > 0)
        {
            bobX += 0.2f * (1f - Mathf.Sin(Mathf.Sqrt(swingProgress * Mathf.Pi) - 0.5f));
            bobY -= 0.2f * (1f - Mathf.Sin(Mathf.Sqrt(swingProgress * Mathf.Pi * 2f) - 0.5f));
        }

        _armRoot.Position = new Vector3(bobX + 0.4f, bobY - 0.76f, bobZ - 0.6f);

        var f3 = Mathf.Sin(swingProgress * swingProgress * Mathf.Pi);
        var f4 = Mathf.Sin(Mathf.Sqrt(swingProgress) * Mathf.Pi);

        _armRoot.RotationDegrees = new Vector3(32f, 96f + f4 * 70f, -45f + f3 * -20f);
    }

    public void Setup(Camera3D camera, double delta)
    {
        _bobRoot.Position = camera.Position;
        _bobRoot.Rotation += Mth.LerpAngle(_bobRoot.Rotation, camera.Rotation, (float)delta * 60f);
        _bobRoot.Rotation = new Vector3(_bobRoot.Rotation.X, Mth.WrapAngle(_bobRoot.Rotation.Y), _bobRoot.Rotation.Z);
    }

    public void Free()
    {
        _armMesh?.Mesh?.Dispose();
        _armMesh?.MaterialOverride?.Dispose();
        _armRoot?.QueueFree();
        _bobRoot?.QueueFree();
    }
}
