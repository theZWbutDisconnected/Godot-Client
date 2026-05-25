namespace TestClient.Source.World.Entities;

public class Rotations
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public Rotations(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Rotations rotations))
            return false;
        
        return X == rotations.X && Y == rotations.Y && Z == rotations.Z;
    }

    public override int GetHashCode()
    {
        var hash = X.GetHashCode();
        hash = hash * 31 + Y.GetHashCode();
        hash = hash * 31 + Z.GetHashCode();
        return hash;
    }
}
