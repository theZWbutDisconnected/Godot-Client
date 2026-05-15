﻿﻿﻿﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using TestClient.Source.Physics;
using TestClient.Source.Render;
using TestClient.Source.World.Entities;
using TestClient.Source.World.Tile;

namespace TestClient.Source.World;

public partial class Level : Node3D
{
    private readonly ConcurrentDictionary<string, ChunkData> _chunks = new();
    private readonly HashSet<Entity> _entities = new();
    private readonly ConcurrentQueue<string> _dirtyChunks = new();
    private readonly ConcurrentDictionary<string, MeshInstance3D> _chunkMeshes = new();
    private bool _isRefreshing = false;

    public void AddChunk(ChunkData chunk)
    {
        string key = ChunkKey(chunk.ChunkX, chunk.ChunkZ);
        _chunks[key] = chunk;
        SetDirty(chunk.ChunkX, chunk.ChunkZ);
        SetDirty(chunk.ChunkX + 1, chunk.ChunkZ);
        SetDirty(chunk.ChunkX - 1, chunk.ChunkZ);
        SetDirty(chunk.ChunkX, chunk.ChunkZ + 1);
        SetDirty(chunk.ChunkX, chunk.ChunkZ - 1);
        
        if (!_isRefreshing)
        {
            _isRefreshing = true;
            Task.Run(() =>
            {
                RefreshDirtyChunks();
                _isRefreshing = false;
            });
        }
    }

    public void SetDirty(int chunkX, int chunkZ)
    {
        string key = ChunkKey(chunkX, chunkZ);
        if (_chunks.ContainsKey(key))
        {
            _dirtyChunks.Enqueue(key);
        }
    }

    public void RefreshDirtyChunks()
    {
        HashSet<string> dirtyKeys = new HashSet<string>();
        while (_dirtyChunks.TryDequeue(out string key))
        {
            dirtyKeys.Add(key);
        }
        
        foreach (string key in dirtyKeys)
        {
            string[] parts = key.Split(',');
            if (parts.Length != 2) continue;
            
            if (!int.TryParse(parts[0], out int cx)) continue;
            if (!int.TryParse(parts[1], out int cz)) continue;
            
            var chunk = GetChunk(cx, cz);
            if (chunk != null)
            {
                BuildChunkMeshAsync(chunk);
            }
        }
    }
    
    private void BuildChunkMeshAsync(ChunkData chunk)
    {
        string key = ChunkKey(chunk.ChunkX, chunk.ChunkZ);
        
        var tessellator = new Tessellator();
        tessellator.Initialize();

        int startX = chunk.ChunkX * ChunkData.Width;
        int startZ = chunk.ChunkZ * ChunkData.Depth;

        for (int y = 0; y < ChunkData.Height; y++)
        {
            for (int x = 0; x < ChunkData.Width; x++)
            {
                for (int z = 0; z < ChunkData.Depth; z++)
                {
                    int worldX = startX + x;
                    int worldY = y;
                    int worldZ = startZ + z;

                    if (chunk.HasBlock(worldX, worldY, worldZ))
                    {
                        Block block = Blocks.GetPreset(GetBlockId(worldX, worldY, worldZ));
                        block.Render(tessellator, this, 0, worldX, worldY, worldZ);
                    }
                }
            }
        }
        
        var meshInstance = tessellator.BuildMeshInstance();
        CallDeferred(nameof(ApplyChunkMesh), key, meshInstance);
    }
    
    private void ApplyChunkMesh(string key, MeshInstance3D newMesh)
    {
        if (_chunkMeshes.TryRemove(key, out var oldMesh))
        {
            if (IsInstanceValid(oldMesh) && oldMesh.GetParent() == this)
            {
                RemoveChild(oldMesh);
            }
            oldMesh?.QueueFree();
        }
        
        if (newMesh != null)
        {
            AddChild(newMesh);
            _chunkMeshes[key] = newMesh;
        }
    }

    public ChunkData? GetChunk(int chunkX, int chunkZ)
    {
        string key = ChunkKey(chunkX, chunkZ);
        _chunks.TryGetValue(key, out var chunk);
        return chunk;
    }

    public void RemoveChunk(int chunkX, int chunkZ)
    {
        _chunks.Remove(ChunkKey(chunkX, chunkZ), out _);
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
                                AABB origin = Blocks.GetPreset(chunk.GetBlockId(x, y, z)).GetCollision();
                                if (origin == null) continue;
                                AABB cube = new AABB(origin);
                                cube.Move(x, y, z);
                                aabbs.Add(cube);
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
    
    public void AddEntity(Entity entity)
    {
        _entities.Add(entity);
    }

    public Entity GetEntityById(int entityId)
    {
        return _entities.FirstOrDefault(e => e.EntityId == entityId);
    }
}