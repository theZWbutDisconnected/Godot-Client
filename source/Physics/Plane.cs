using System;
using System.Collections.Generic;

namespace TestClient.Source.Physics;


public class Plane : IEnumerable<Direction>
{
    public static readonly Plane HORIZONTAL = new Plane();
    public static readonly Plane VERTICAL = new Plane();

    private Plane() { }

    public Direction[] Facings => this == HORIZONTAL ? new[] { Direction.NORTH, Direction.EAST, Direction.SOUTH, Direction.WEST } : new[] { Direction.UP, Direction.DOWN };

    public Direction Random(Random rand)
    {
        var facings = Facings;
        return facings[rand.Next(facings.Length)];
    }

    public bool Matches(Direction facing) => facing != null && facing.Axis.Plane == this;

    public IEnumerator<Direction> GetEnumerator()
    {
        foreach (var f in Facings) yield return f;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}