using System;
using System.Collections.Generic;

namespace TestClient.Source.Physics;

public class Axis : IStringSerializable
{
    public static readonly Axis X = new Axis("x", Plane.HORIZONTAL);
    public static readonly Axis Y = new Axis("y", Plane.VERTICAL);
    public static readonly Axis Z = new Axis("z", Plane.HORIZONTAL);

    private static readonly Dictionary<string, Axis> _axisNameLookup = new Dictionary<string, Axis>();
    static Axis()
    {
        _axisNameLookup[X._axisName] = X;
        _axisNameLookup[Y._axisName] = Y;
        _axisNameLookup[Z._axisName] = Z;
    }

    private readonly string _axisName;
    private Axis(string name, Plane plane)
    {
        _axisName = name;
        Plane = plane;
    }

    public string Name => _axisName;
    public Plane Plane { get; }

    public bool IsVertical => Plane == Plane.VERTICAL;
    public bool IsHorizontal => Plane == Plane.HORIZONTAL;

    public static Axis ByName(string name) =>
        name == null ? null : _axisNameLookup.TryGetValue(name.ToLowerInvariant(), out var a) ? a : null;

    public override string ToString() => _axisName;
}