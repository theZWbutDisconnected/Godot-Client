﻿using System;
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
    private readonly Dictionary<ChunkCoordIntPair, ChunkData> _chunks = new();
    private readonly HashSet<Entity> _entities = new();
    private readonly List<(ChunkCoordIntPair coord, int priority)> _dirtyChunks = new();
    private readonly Dictionary<ChunkCoordIntPair, MeshInstance3D> _chunkMeshes = new();
    private readonly object _lockObj = new();
    private bool _isRefreshing = false;

    public void AddChunk(ChunkData chunk)
    {
        var key = new ChunkCoordIntPair(chunk.ChunkX, chunk.ChunkZ);
        lock (_lockObj)
        {
            _chunks[key] = chunk;
        }
        SetDirty(key);
        SetDirty(new ChunkCoordIntPair(chunk.ChunkX + 1, chunk.ChunkZ));
        SetDirty(new ChunkCoordIntPair(chunk.ChunkX - 1, chunk.ChunkZ));
        SetDirty(new ChunkCoordIntPair(chunk.ChunkX, chunk.ChunkZ + 1));
        SetDirty(new ChunkCoordIntPair(chunk.ChunkX, chunk.ChunkZ - 1));
    }

    public void SetDirty(ChunkCoordIntPair chunkCoord, int priority = -1)
    {
        lock (_lockObj)
        {
            if (!_chunks.ContainsKey(chunkCoord)) return;
            _dirtyChunks.RemoveAll(item => item.coord.Equals(chunkCoord));
            if (priority < 0)
            {
                _dirtyChunks.Add((chunkCoord, priority));
            }
            else
            {
                var insertIndex = Math.Min(priority, _dirtyChunks.Count);
                _dirtyChunks.Insert(insertIndex, (chunkCoord, priority));
            }
        }
    }

    public void RefreshDirtyChunks()
    {
        List<ChunkCoordIntPair> dirtyKeys;
        
        lock (_lockObj)
        {
            dirtyKeys = _dirtyChunks.Select(item => item.coord).ToList();
            _dirtyChunks.Clear();
        }

        foreach (var key in dirtyKeys)
        {
            var chunk = GetChunk(key.ChunkXPos, key.ChunkZPos);
            if (chunk != null) BuildChunkMeshAsync(chunk);
        }
    }

    private void BuildChunkMeshAsync(ChunkData chunk)
    {
        var key = new ChunkCoordIntPair(chunk.ChunkX, chunk.ChunkZ);

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

            var pos = new BlockPos(worldX, worldY, worldZ);
            if (chunk.HasBlock(worldX, worldY, worldZ))
            {
                var block = Blocks.GetPreset(GetBlockId(pos));
                block.Render(tessellator, this, 0, pos);
            }
        }

        var meshInstance = tessellator.BuildMeshInstance();
        CallDeferred(nameof(ApplyChunkMesh), chunk.ChunkX, chunk.ChunkZ, meshInstance);
    }

    private void ApplyChunkMesh(int chunkX, int chunkZ, MeshInstance3D newMesh)
    {
        var key = new ChunkCoordIntPair(chunkX, chunkZ);
        lock (_lockObj)
        {
            if (_chunkMeshes.TryGetValue(key, out var oldMesh))
            {
                _chunkMeshes.Remove(key);
                RemoveChild(oldMesh);
                oldMesh?.QueueFree();
            }

            if (newMesh != null)
            {
                AddChild(newMesh);
                _chunkMeshes[key] = newMesh;
            }
        }
    }

    #nullable enable
    public ChunkData? GetChunk(int chunkX, int chunkZ)
    {
        var key = new ChunkCoordIntPair(chunkX, chunkZ);
        ChunkData chunk;
        lock (_lockObj)
        {
            _chunks.TryGetValue(key, out var c);
            chunk = c;
        }
        return chunk;
    }
    #nullable disable

    public bool HasChunk(int chunkX, int chunkZ)
    {
        return GetChunk(chunkX, chunkZ) != null;
    }

    public void RemoveChunk(int chunkX, int chunkZ)
    {
        lock (_lockObj)
        {
            _chunks.Remove(new ChunkCoordIntPair(chunkX, chunkZ));
        }
    }

    public int GetBlockId(BlockPos pos)
    {
        var cx = ChunkData.WorldToChunk(pos.X);
        var cz = ChunkData.WorldToChunk(pos.Z);
        var chunk = GetChunk(cx, cz);
        if (chunk == null) return 0;
        return chunk.GetBlockId(pos.X, pos.Y, pos.Z);
    }

    public int GetMetadata(BlockPos pos)
    {
        var cx = ChunkData.WorldToChunk(pos.X);
        var cz = ChunkData.WorldToChunk(pos.Z);
        var chunk = GetChunk(cx, cz);
        if (chunk == null) return 0;
        return chunk.GetMetadata(pos.X, pos.Y, pos.Z);
    }

    public bool HasBlock(BlockPos pos)
	{
		var cx = ChunkData.WorldToChunk(pos.X);
		var cz = ChunkData.WorldToChunk(pos.Z);
		var chunk = GetChunk(cx, cz);
		if (chunk == null) return false;
		return chunk.HasBlock(pos.X, pos.Y, pos.Z);
	}

	public void SetBlock(BlockPos pos, int blockId, int metadata = 0)
	{
		var cx = ChunkData.WorldToChunk(pos.X);
		var cz = ChunkData.WorldToChunk(pos.Z);
		var chunk = GetChunk(cx, cz);
		if (chunk != null)
		{
			chunk.SetBlock(pos.X, pos.Y, pos.Z, blockId, metadata);
			SetDirty(new ChunkCoordIntPair(cx, cz), 0);
            if (cx == 0) SetDirty(new ChunkCoordIntPair(cx - 1, cz), 1);
            if (cz == 0) SetDirty(new ChunkCoordIntPair(cx, cz - 1), 1);
            if (cx == 15) SetDirty(new ChunkCoordIntPair(cx + 1, cz), 1);
            if (cz == 15) SetDirty(new ChunkCoordIntPair(cx, cz + 1), 1);
		}
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
                    var pos = new BlockPos(x, y, z);
                    var origin = Blocks.GetPreset(GetBlockId(pos)).GetCollision();
                    if (origin == null) continue;
                    var cube = new AABB(origin);
                    cube.Move(x, y, z);
                    aabbs.Add(cube);
                }
        }

        return aabbs;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

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

    public bool IsBlockLoaded(BlockPos pos)
    {
        return IsBlockLoaded(pos, true);
    }

    public bool IsBlockLoaded(BlockPos pos, bool allowEmpty)
    {
        return IsChunkLoaded(pos.X >> 4, pos.Z >> 4, allowEmpty);
    }

    protected bool IsChunkLoaded(int x, int z, bool allowEmpty)
    {
        return HasChunk(x, z);
    }
}