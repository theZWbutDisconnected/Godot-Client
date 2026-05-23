namespace TestClient.Source;

public class Capabilities
{
    public bool DisableDamage { get; set; }
    public bool IsFlying { get; set; }
    public bool AllowFlying { get; set; }
    public bool IsCreativeMode { get; set; }
    public bool AllowEdit { get; set; } = true;

    public float FlySpeed { get; set; } = 0.05f;

    public float WalkSpeed { get; set; } = 0.1f;
}