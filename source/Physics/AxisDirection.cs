namespace TestClient.Source.Physics;

public class AxisDirection
{
    public static readonly AxisDirection POSITIVE = new AxisDirection(1, "Towards positive");
    public static readonly AxisDirection NEGATIVE = new AxisDirection(-1, "Towards negative");

    private AxisDirection(int offset, string description)
    {
        Offset = offset;
        Description = description;
    }

    public int Offset { get; }
    public string Description { get; }

    public override string ToString() => Description;
}