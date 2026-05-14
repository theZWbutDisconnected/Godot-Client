using System.Collections.Generic;
using Godot;
using TestClient.Source.Physics;

namespace TestClient.Source.World;

public partial class Level : Node3D
{
    public List<AABB> GetCubes(AABB expand)
    {
        var aabbs = new List<AABB>();
        return aabbs;
    }
}