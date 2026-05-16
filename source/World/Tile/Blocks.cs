namespace TestClient.Source.World.Tile;

public class Blocks
{
    private static readonly Block[] Presets = new Block[256];
    public static readonly Block Empty = new AirTile(0);
    public static readonly Block Rock = new(1, 1);
    public static readonly Block Grass = new GrassBlock(2);
    public static readonly Block Dirt = new(3, 2);
    public static readonly Block Bedrock = new(7, 17);
    public static readonly Block Sand = new(12, 18);
    public static readonly Block OakLog = new LogBlock(17);
    public static readonly Block OakLeaves = new Leaves(18);
    
    public static Block GetPreset(int id)
    {
        Block block = Presets[id];
        if (block == null) block = Empty;
        return block;
    }
    
    public static void SetPreset(int id, Block block)
    {
        Presets[id] = block;
    }
}