namespace TestClient.Source.World.Tile;

using TestClient.Source.Render;

public class Blocks
{
    private static readonly Block[] Presets = new Block[256];
    public static readonly Block Empty = new AirTile(0);
    public static readonly Block Rock = new(1, TextureAtlas.Index("stone"));
    public static readonly Block Grass = new GrassBlock(2);
    public static readonly Block Dirt = new(3, TextureAtlas.Index("dirt"));
    public static readonly Block Bush = new Bush(6, TextureAtlas.Index("sapling_oak"));
    public static readonly Block Bedrock = new(7, TextureAtlas.Index("bedrock"));
    public static readonly Block FlowingWater = new LiquidTile(8, LiquidType.Water);
    public static readonly Block Water = new LiquidTile(9, LiquidType.Water);
    public static readonly Block FlowingLava = new LiquidTile(10, LiquidType.Lava);
    public static readonly Block Lava = new LiquidTile(11, LiquidType.Lava);
    public static readonly Block Sand = new(12, TextureAtlas.Index("sand"));
    public static readonly Block Gravel = new(13, TextureAtlas.Index("gravel"));
    public static readonly Block OakLog = new LogBlock(17);
    public static readonly Block OakLeaves = new Leaves(18);
    public static readonly Block TallGrass = new Bush(31, TextureAtlas.Index("tallgrass"));
    public static readonly Block Flower = new Bush(38, TextureAtlas.Index("flower_rose"));
    public static readonly Block Clay = new(82, TextureAtlas.Index("clay"));
    public static readonly Block DoubleGrass = new DoubleBush(175, TextureAtlas.Index("tallgrass"));

    public static Block GetPreset(int id)
    {
        var block = Presets[id];
        if (block == null) block = Rock;
        return block;
    }

    public static void SetPreset(int id, Block block)
    {
        Presets[id] = block;
    }
}