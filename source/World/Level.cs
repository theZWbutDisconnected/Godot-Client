using System.Collections.Generic;
using Godot;
using TestClient.Source.Physics;

namespace TestClient.Source.World;

public partial class Level : Node3D
{
    private readonly Dictionary<string, ChunkData> _chunks = new();

    public ChunkData GetOrCreateChunk(int chunkX, int chunkZ)
    {
        string key = ChunkKey(chunkX, chunkZ);
        if (_chunks.TryGetValue(key, out var chunk))
            return chunk;

        chunk = new ChunkData(chunkX, chunkZ);
        _chunks[key] = chunk;
        return chunk;
    }

    public ChunkData? GetChunk(int chunkX, int chunkZ)
    {
        string key = ChunkKey(chunkX, chunkZ);
        return _chunks.TryGetValue(key, out var chunk) ? chunk : null;
    }

    public void RemoveChunk(int chunkX, int chunkZ)
    {
        _chunks.Remove(ChunkKey(chunkX, chunkZ));
    }

    public int GetBlockId(int worldX, int worldY, int worldZ)
    {
        int cx = ChunkData.WorldToChunk(worldX);
        int cz = ChunkData.WorldToChunk(worldZ);
        var chunk = GetChunk(cx, cz);
        if (chunk == null) return 0;
        return chunk.GetBlockId(worldX, worldY, worldZ);
    }

    public int GetMetadata(int worldX, int worldY, int worldZ)
    {
        int cx = ChunkData.WorldToChunk(worldX);
        int cz = ChunkData.WorldToChunk(worldZ);
        var chunk = GetChunk(cx, cz);
        if (chunk == null) return 0;
        return chunk.GetMetadata(worldX, worldY, worldZ);
    }

    public bool HasBlock(int worldX, int worldY, int worldZ)
    {
        int cx = ChunkData.WorldToChunk(worldX);
        int cz = ChunkData.WorldToChunk(worldZ);
        var chunk = GetChunk(cx, cz);
        if (chunk == null) return false;
        return chunk.HasBlock(worldX, worldY, worldZ);
    }

    public List<AABB> GetCubes(AABB expand)
    {
        var aabbs = new List<AABB>();

        int minX = Mathf.FloorToInt(expand.X0);
        int minY = Mathf.FloorToInt(expand.Y0);
        int minZ = Mathf.FloorToInt(expand.Z0);
        int maxX = Mathf.FloorToInt(expand.X1);
        int maxY = Mathf.FloorToInt(expand.Y1);
        int maxZ = Mathf.FloorToInt(expand.Z1);

        int minCX = ChunkData.WorldToChunk(minX);
        int maxCX = ChunkData.WorldToChunk(maxX);
        int minCZ = ChunkData.WorldToChunk(minZ);
        int maxCZ = ChunkData.WorldToChunk(maxZ);

        for (int cx = minCX; cx <= maxCX; cx++)
        {
            for (int cz = minCZ; cz <= maxCZ; cz++)
            {
                var chunk = GetChunk(cx, cz);
                if (chunk == null) continue;

                int lxMin = Mathf.Max(minX, ChunkData.LocalToWorld(0, cx));
                int lxMax = Mathf.Min(maxX, ChunkData.LocalToWorld(15, cx));
                int lzMin = Mathf.Max(minZ, ChunkData.LocalToWorld(0, cz));
                int lzMax = Mathf.Min(maxZ, ChunkData.LocalToWorld(15, cz));

                for (int x = lxMin; x <= lxMax; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        for (int z = lzMin; z <= lzMax; z++)
                        {
                            if (chunk.HasBlock(x, y, z))
                            {
                                aabbs.Add(new AABB(x, y, z, x + 1, y + 1, z + 1));
                            }
                        }
                    }
                }
            }
        }

        return aabbs;
    }

    private static string ChunkKey(int cx, int cz) => $"{cx},{cz}";

    public bool IsLit(int p0, int p1, int p2)
    {
        return true;
    }
}
