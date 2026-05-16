namespace TestClient.Source.World.Tile;

public class Leaves : Block
{
    public Leaves(int id) : base(id, 22)
    {
    }

    public override bool IsOpaque()
    {
        return false;
    }
}