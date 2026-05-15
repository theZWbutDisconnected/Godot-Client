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
        var key = ChunkKey(chunk.ChunkX, chunk.ChunkZ);
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
        var key = ChunkKey(chunkX, chunkZ);
        if (_chunks.ContainsKey(key)) _dirtyChunks.Enqueue(key);
    }

    public void RefreshDirtyChunks()
    {
        var dirtyKeys = new HashSet<string>();
        while (_dirtyChunks.TryDequeue(out var key)) dirtyKeys.Add(key);

        foreach (var key in dirtyKeys)
        {
            var parts = key.Split(',');
            if (parts.Length != 2) continue;

            if (!int.TryParse(parts[0], out var cx)) continue;
            if (!int.TryParse(parts[1], out var cz)) continue;

            var chunk = GetChunk(cx, cz);
            if (chunk != null) BuildChunkMeshAsync(chunk);
        }
    }

    private void BuildChunkMeshAsync(ChunkData chunk)
    {
        var key = ChunkKey(chunk.ChunkX, chunk.ChunkZ);

        var tessellator = new Tessellator();
        tessellator.Initialize();

        var startX = chunk.ChunkX * ChunkData.Width;
        var startZ = chunk.ChunkZ * ChunkData.Depth;

        for (var y = 0; y < ChunkData.Height; y++)
        for (var x = 0; x < ChunkData.Width; x++)
        for (var z = 0; z < ChunkData.Depth; z++)
        {
            var worldX = startX + x;
            var worldY = y;
            var worldZ = startZ + z;

            if (chunk.HasBlock(worldX, worldY, worldZ))
            {
                var block = Blocks.GetPreset(GetBlockId(worldX, worldY, worldZ));
                block.Render(tessellator, this, 0, worldX, worldY, worldZ);
            }
        }

        var meshInstance = tessellator.BuildMeshInstance();
        CallDeferred(nameof(ApplyChunkMesh), key, meshInstance);
    }

    private void ApplyChunkMesh(string key, MeshInstance3D newMesh)
    {
        if (_chunkMeshes.TryRemove(key, out var oldMesh))
        {
            if (IsInstanceValid(oldMesh) && oldMesh.GetParent() == this) RemoveChild(oldMesh);
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
        var key = ChunkKey(chunkX, chunkZ);
        _chunks.TryGetValue(key, out var chunk);
        return chunk;
    }

    public void RemoveChunk(int chunkX, int chunkZ)
    {
        _chunks.Remove(ChunkKey(chunkX, chunkZ), out _);
    }

    public int GetBlockId(int worldX, int worldY, int worldZ)
    {
        var cx = ChunkData.WorldToChunk(worldX);
        var cz = ChunkData.WorldToChunk(worldZ);
        var chunk = GetChunk(cx, cz);
        if (chunk == null) return 0;
        return chunk.GetBlockId(worldX, worldY, worldZ);
    }

    public int GetMetadata(int worldX, int worldY, int worldZ)
    {
        var cx = ChunkData.WorldToChunk(worldX);
        var cz = ChunkData.WorldToChunk(worldZ);
        var chunk = GetChunk(cx, cz);
        if (chunk == null) return 0;
        return chunk.GetMetadata(worldX, worldY, worldZ);
    }

    public bool HasBlock(int worldX, int worldY, int worldZ)
    {
        var cx = ChunkData.WorldToChunk(worldX);
        var cz = ChunkData.WorldToChunk(worldZ);
        var chunk = GetChunk(cx, cz);
        if (chunk == null) return false;
        return chunk.HasBlock(worldX, worldY, worldZ);
    }

    public List<AABB> GetCubes(AABB expand)
    {
        var aabbs = new List<AABB>();

        var minX = Mathf.FloorToInt(expand.X0);
        var minY = Mathf.FloorToInt(expand.Y0);
        var minZ = Mathf.FloorToInt(expand.Z0);
        var maxX = Mathf.FloorToInt(expand.X1);
        var maxY = Mathf.FloorToInt(expand.Y1);
        var maxZ = Mathf.FloorToInt(expand.Z1);

        var minCX = ChunkData.WorldToChunk(minX);
        var maxCX = ChunkData.WorldToChunk(maxX);
        var minCZ = ChunkData.WorldToChunk(minZ);
        var maxCZ = ChunkData.WorldToChunk(maxZ);

        for (var cx = minCX; cx <= maxCX; cx++)
        for (var cz = minCZ; cz <= maxCZ; cz++)
        {
            var chunk = GetChunk(cx, cz);
            if (chunk == null) continue;

            var lxMin = Mathf.Max(minX, ChunkData.LocalToWorld(0, cx));
            var lxMax = Mathf.Min(maxX, ChunkData.LocalToWorld(15, cx));
            var lzMin = Mathf.Max(minZ, ChunkData.LocalToWorld(0, cz));
            var lzMax = Mathf.Min(maxZ, ChunkData.LocalToWorld(15, cz));

            for (var x = lxMin; x <= lxMax; x++)
            for (var y = minY; y <= maxY; y++)
            for (var z = lzMin; z <= lzMax; z++)
                if (chunk.HasBlock(x, y, z))
                {
                    var origin = Blocks.GetPreset(chunk.GetBlockId(x, y, z)).GetCollision();
                    if (origin == null) continue;
                    var cube = new AABB(origin);
                    cube.Move(x, y, z);
                    aabbs.Add(cube);
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