using System.Collections.Generic;

namespace TestClient.Source.Physics;

public class Axis : IStringSerializable
{
    public static readonly Axis X = new("x", Plane.HORIZONTAL);
    public static readonly Axis Y = new("y", Plane.VERTICAL);
    public static readonly Axis Z = new("z", Plane.HORIZONTAL);

    private static readonly Dictionary<string, Axis> _axisNameLookup = new();

    static Axis()
    {
        _axisNameLookup[X.Name] = X;
        _axisNameLookup[Y.Name] = Y;
        _axisNameLookup[Z.Name] = Z;
    }

    private Axis(string name, Plane plane)
    {
        Name = name;
        Plane = plane;
    }

    public Plane Plane { get; }

    public bool IsVertical => Plane == Plane.VERTICAL;
    public bool IsHorizontal => Plane == Plane.HORIZONTAL;

    public string Name { get; }

    public static Axis ByName(string name)
    {
        return name == null ? null : _axisNameLookup.TryGetValue(name.ToLowerInvariant(), out var a) ? a : null;
    }

    public override string ToString()
    {
        return Name;
    }
}