using System;

namespace TestClient.Source.Physics;

public class Vec3i : IEquatable<Vec3i>
{
    public Vec3i(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vec3i(double x, double y, double z) : this((int)Math.Floor(x), (int)Math.Floor(y), (int)Math.Floor(z))
    {
    }

    public int X { get; }
    public int Y { get; }
    public int Z { get; }

    public bool Equals(Vec3i other)
    {
        return other != null && X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Vec3i);
    }

    public override int GetHashCode()
    {
        return (X, Y, Z).GetHashCode();
    }

    public static bool operator ==(Vec3i left, Vec3i right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Vec3i left, Vec3i right)
    {
        return !Equals(left, right);
    }
}