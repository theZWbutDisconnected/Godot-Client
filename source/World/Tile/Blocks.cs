namespace TestClient.Source.World.Tile;

public class Blocks
{
    public static readonly Block[] Presets = new Block[256];
    public static readonly Block Rock = new(1, 1);
    public static readonly Block Grass = new GrassBlock(2);
    public static readonly Block Dirt = new(3, 2);
    public static readonly Block Sand = new(12, 18);
}