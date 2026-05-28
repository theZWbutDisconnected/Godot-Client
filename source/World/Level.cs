using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using TestClient.Source.Physics;
using TestClient.Source.Render;
using TestClient.Source.World.Entities;
using TestClient.Source.World.Tile;

namespace TestClient.Source.World;

public partial class Level : Node3D
{
	private readonly System.Collections.Generic.Dictionary<ChunkCoordIntPair, List<MeshInstance3D>>
		_chunkMeshes = new();

	private readonly System.Collections.Generic.Dictionary<ChunkCoordIntPair, ChunkData> _chunks = new();
	private readonly List<(ChunkCoordIntPair coord, int priority)> _dirtyChunks = new();
	private readonly HashSet<Entity> _entities = new();

	public Entity[] Entities
	{
		get
		{
			lock (_lockObj)
			{
				var arr = new Entity[_entities.Count];
				_entities.CopyTo(arr);
				return arr;
			}
		}
	}

	private readonly object _lockObj = new();
	private bool _isRefreshing;

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
		var startX = chunk.ChunkX * ChunkData.Width;
		var startZ = chunk.ChunkZ * ChunkData.Depth;

		var groups = new System.Collections.Generic.Dictionary<MeshGroupKey, Tessellator>();

		for (var y = 0; y < ChunkData.Height; y++)
		{
			var sectionIdx = y >> 4;
			var section = chunk.GetSection(sectionIdx);

			if (section == null || section.IsEmpty)
			{
				y = (sectionIdx + 1) * ChunkSection.Height - 1;
				continue;
			}

			var data = section.GetData();
			var localY = y % 16;

			for (var x = 0; x < ChunkData.Width; x++)
			for (var z = 0; z < ChunkData.Depth; z++)
			{
				var idx = localY * ChunkSection.Width * ChunkSection.Depth + z * ChunkSection.Width + x;
				var blockRaw = data[idx];
				if (blockRaw == 0)
					continue;

				var blockId = (blockRaw >> 4) & 0xFFF;

				var worldX = startX + x;
				var worldY = y;
				var worldZ = startZ + z;

				var pos = new BlockPos(worldX, worldY, worldZ);
				var block = Blocks.GetPreset(blockId);

				var isLiquid = block is LiquidTile;
				var texId = block.TexId;
				var animated = TextureAtlas.IsAnimated(texId);

				var groupKey = new MeshGroupKey(isLiquid, animated ? texId : -1);

				if (!groups.TryGetValue(groupKey, out var tess))
				{
					tess = new Tessellator();
					tess.Initialize();
					groups[groupKey] = tess;
				}

				block.Render(tess, this, pos);
			}
		}

		var meshes = new Array<MeshInstance3D>();
		foreach (var (groupKey, tess) in groups)
		{
			Material material;

			if (groupKey.TexIndex >= 0)
			{
				var animData = TextureAtlas.GetAnimData(groupKey.TexIndex);
				if (animData.HasValue)
					material = Tessellator.GetOrCreateAnimMaterial(
						groupKey.TexIndex, animData.Value, groupKey.IsLiquid);
				else
					material = groupKey.IsLiquid ? Tessellator.GetLiquidMaterial() : Tessellator.GetSolidMaterial();
			}
			else
			{
				material = groupKey.IsLiquid ? Tessellator.GetLiquidMaterial() : Tessellator.GetSolidMaterial();
			}

			var meshInstance = tess.BuildMeshInstance(material);
			if (meshInstance != null)
				meshes.Add(meshInstance);
		}

		CallDeferred(nameof(ApplyChunkMesh), chunk.ChunkX, chunk.ChunkZ, meshes);
	}

	private void ApplyChunkMesh(int chunkX, int chunkZ, Array<MeshInstance3D> meshes)
	{
		var key = new ChunkCoordIntPair(chunkX, chunkZ);
		lock (_lockObj)
		{
			if (_chunkMeshes.TryGetValue(key, out var oldMeshes))
			{
				_chunkMeshes.Remove(key);
				if (oldMeshes != null)
					foreach (var old in oldMeshes)
						if (old != null && IsInstanceValid(old))
						{
							RemoveChild(old);
							old.QueueFree();
						}
			}

			if (meshes != null && meshes.Count > 0)
			{
				foreach (var mesh in meshes)
					if (mesh != null)
						AddChild(mesh);
				_chunkMeshes[key] = new List<MeshInstance3D>(meshes);
			}
		}
	}

#nullable enable
	public ChunkData? GetChunk(int chunkX, int chunkZ)
	{
		var key = new ChunkCoordIntPair(chunkX, chunkZ);
		ChunkData? chunk;
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
			var lx = ChunkData.WorldToLocal(pos.X, cx);
			var lz = ChunkData.WorldToLocal(pos.Z, cz);
			if (lx == 0) SetDirty(new ChunkCoordIntPair(cx - 1, cz), 1);
			if (lz == 0) SetDirty(new ChunkCoordIntPair(cx, cz - 1), 1);
			if (lx == 15) SetDirty(new ChunkCoordIntPair(cx + 1, cz), 1);
			if (lz == 15) SetDirty(new ChunkCoordIntPair(cx, cz + 1), 1);
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
					var origin = Blocks.GetPreset(GetBlockId(pos)).GetCollision(GetMetadata(pos));
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

	public bool IsBlockOpaque(int x, int y, int z)
	{
		var cx = ChunkData.WorldToChunk(x);
		var cz = ChunkData.WorldToChunk(z);
		var chunk = GetChunk(cx, cz);
		if (chunk == null) return false;

		var blockId = chunk.GetBlockId(x, y, z);
		if (blockId == 0) return false;

		return Blocks.GetPreset(blockId).IsOpaque();
	}

	public void AddEntity(Entity entity)
	{
		_entities.Add(entity);
	}

	public void AddEntity(int id, Entity entity)
	{
		entity.EntityId = id;
		_entities.Add(entity);
	}

	public Entity GetEntityById(int entityId)
	{
		Entity target = null;
		foreach (var e in _entities)
			if (e.EntityId == entityId)
				target = e;

		return target;
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

	public void Tick()
	{
		foreach (var i in _entities)
			if (!i.Removed)
			{
				++i.TicksExisted;
				i.Tick();
			}
	}

	public void RemoveEntity(int packetInEntityId)
	{
		var entity = GetEntityById(packetInEntityId);
		if (entity == null) return;
		entity.Remove();
		_entities.Remove(entity);
	}

	private struct MeshGroupKey : IEquatable<MeshGroupKey>
	{
		public readonly bool IsLiquid;
		public readonly int TexIndex;

		public MeshGroupKey(bool isLiquid, int texIndex)
		{
			IsLiquid = isLiquid;
			TexIndex = texIndex;
		}

		public bool Equals(MeshGroupKey other)
		{
			return IsLiquid == other.IsLiquid && TexIndex == other.TexIndex;
		}

		public override bool Equals(object obj)
		{
			return obj is MeshGroupKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(IsLiquid, TexIndex);
		}
	}
}
