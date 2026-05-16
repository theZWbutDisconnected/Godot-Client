using System;
using System.Collections;
using System.Collections.Generic;

namespace TestClient.Source.Physics;

public class Plane : IEnumerable<Direction>
{
    public static readonly Plane HORIZONTAL = new();
    public static readonly Plane VERTICAL = new();

    private Plane()
    {
    }

    public Direction[] Facings => this == HORIZONTAL
        ? new[] { Direction.NORTH, Direction.EAST, Direction.SOUTH, Direction.WEST }
        : new[] { Direction.UP, Direction.DOWN };

    public IEnumerator<Direction> GetEnumerator()
    {
        foreach (var f in Facings) yield return f;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Direction Random(Random rand)
    {
        var facings = Facings;
        return facings[rand.Next(facings.Length)];
    }

    public bool Matches(Direction facing)
    {
        return facing != null && facing.Axis.Plane == this;
    }
}