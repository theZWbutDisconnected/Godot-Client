using System;
using System.Collections.Generic;
using Godot;

namespace TestClient.Source.Physics;

public interface IStringSerializable
{
    string Name { get; }
}

public class Direction : IStringSerializable
{
    public static readonly Direction DOWN = new(0, 1, -1, "down", AxisDirection.NEGATIVE, Axis.Y, new Vec3i(0, -1, 0));
    public static readonly Direction UP = new(1, 0, -1, "up", AxisDirection.POSITIVE, Axis.Y, new Vec3i(0, 1, 0));
    public static readonly Direction NORTH = new(2, 3, 2, "north", AxisDirection.NEGATIVE, Axis.Z, new Vec3i(0, 0, -1));
    public static readonly Direction SOUTH = new(3, 2, 0, "south", AxisDirection.POSITIVE, Axis.Z, new Vec3i(0, 0, 1));
    public static readonly Direction WEST = new(4, 5, 1, "west", AxisDirection.NEGATIVE, Axis.X, new Vec3i(-1, 0, 0));
    public static readonly Direction EAST = new(5, 4, 3, "east", AxisDirection.POSITIVE, Axis.X, new Vec3i(1, 0, 0));

    private static readonly Direction[] _values = new Direction[6];
    private static readonly Direction[] _horizontals = new Direction[4];
    private static readonly Dictionary<string, Direction> _nameLookup = new();

    private readonly int _oppositeIndex;

    static Direction()
    {
        foreach (var f in new[] { DOWN, UP, NORTH, SOUTH, WEST, EAST })
        {
            _values[f.Index] = f;
            if (f.Axis.IsHorizontal)
                _horizontals[f.HorizontalIndex] = f;
            _nameLookup[f.Name] = f;
        }
    }

    private Direction(int index, int oppositeIndex, int horizontalIndex, string name,
        AxisDirection axisDirection, Axis axis, Vec3i directionVec)
    {
        Index = index;
        _oppositeIndex = oppositeIndex;
        HorizontalIndex = horizontalIndex;
        Name = name;
        AxisDirection = axisDirection;
        Axis = axis;
        DirectionVec = directionVec;
    }

    public int Index { get; }

    public int HorizontalIndex { get; }

    public AxisDirection AxisDirection { get; }
    public Axis Axis { get; }
    public Vec3i DirectionVec { get; }

    public Direction Opposite => _values[_oppositeIndex];

    public int OffsetX => Axis == Axis.X ? AxisDirection.Offset : 0;
    public int OffsetY => Axis == Axis.Y ? AxisDirection.Offset : 0;
    public int OffsetZ => Axis == Axis.Z ? AxisDirection.Offset : 0;
    public string Name { get; }

    public Direction RotateAround(Axis axis)
    {
        if (axis == Axis.X) return this != WEST && this != EAST ? RotateX() : this;
        if (axis == Axis.Y) return this != UP && this != DOWN ? RotateY() : this;
        if (axis == Axis.Z) return this != NORTH && this != SOUTH ? RotateZ() : this;
        throw new InvalidOperationException($"Unable to get CW facing for axis {axis}");
    }

    public Direction RotateY()
    {
        if (this == NORTH) return EAST;
        if (this == EAST) return SOUTH;
        if (this == SOUTH) return WEST;
        if (this == WEST) return NORTH;
        throw new InvalidOperationException($"Unable to get Y-rotated facing of {this}");
    }

    private Direction RotateX()
    {
        if (this == NORTH) return DOWN;
        if (this == SOUTH) return UP;
        if (this == UP) return NORTH;
        if (this == DOWN) return SOUTH;
        throw new InvalidOperationException($"Unable to get X-rotated facing of {this}");
    }

    private Direction RotateZ()
    {
        if (this == EAST) return DOWN;
        if (this == WEST) return UP;
        if (this == UP) return EAST;
        if (this == DOWN) return WEST;
        throw new InvalidOperationException($"Unable to get Z-rotated facing of {this}");
    }

    public Direction RotateYCCW()
    {
        if (this == NORTH) return WEST;
        if (this == EAST) return NORTH;
        if (this == SOUTH) return EAST;
        if (this == WEST) return SOUTH;
        throw new InvalidOperationException($"Unable to get CCW facing of {this}");
    }

    public static Direction ByName(string name)
    {
        return name == null ? null : _nameLookup.TryGetValue(name.ToLowerInvariant(), out var f) ? f : null;
    }

    public static Direction GetFront(int index)
    {
        return _values[Mathf.Abs(index % _values.Length)];
    }

    public static Direction GetHorizontal(int index)
    {
        return _horizontals[Mathf.Abs(index % _horizontals.Length)];
    }

    public static Direction FromAngle(double angle)
    {
        return GetHorizontal(Mathf.FloorToInt(angle / 90.0 + 0.5) & 3);
    }

    public static Direction Random(Random rand)
    {
        return _values[rand.Next(_values.Length)];
    }

    public static Direction GetFacingFromVector(float x, float y, float z)
    {
        var best = NORTH;
        var bestDot = float.MinValue;
        foreach (var f in _values)
        {
            var dot = x * f.DirectionVec.X + y * f.DirectionVec.Y + z * f.DirectionVec.Z;
            if (dot > bestDot)
            {
                bestDot = dot;
                best = f;
            }
        }

        return best;
    }

    public static Direction GetFacingFromAxis(AxisDirection axisDirection, Axis axis)
    {
        foreach (var f in _values)
            if (f.AxisDirection == axisDirection && f.Axis == axis)
                return f;
        throw new ArgumentException($"No such direction: {axisDirection} {axis}");
    }

    public override string ToString()
    {
        return Name;
    }
}