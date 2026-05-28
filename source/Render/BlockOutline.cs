using Godot;
using TestClient.Source.Physics;
using TestClient.Source.World;
using TestClient.Source.World.Tile;

namespace TestClient.Source.Render;

public class BlockOutline
{
	private readonly MeshInstance3D _outlineMesh;
	private readonly ImmediateMesh _immediateMesh;
	private readonly StandardMaterial3D _material;

	private bool _visible;
	private BlockPos _lastPos;
	private double _lastMinX, _lastMinY, _lastMinZ;
	private double _lastMaxX, _lastMaxY, _lastMaxZ;

	private const float Expand = 0.002f;

	public BlockOutline()
	{
		_outlineMesh = new MeshInstance3D
		{
			Name = "BlockOutline",
			Visible = false
		};

		_material = new StandardMaterial3D
		{
			ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
			VertexColorUseAsAlbedo = true,
			AlbedoColor = new Color(0, 0, 0),
			Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
			DepthDrawMode = BaseMaterial3D.DepthDrawModeEnum.Always,
			NoDepthTest = false
		};

		_immediateMesh = new ImmediateMesh();
		_outlineMesh.Mesh = _immediateMesh;
		_outlineMesh.MaterialOverride = _material;
		_outlineMesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
	}

	public Node3D Node => _outlineMesh;

	public void Update(Level level, MoveObject mop)
	{
		if (mop == null || mop.Type != MoveObjectType.Block)
		{
			if (_visible)
			{
				_outlineMesh.Visible = false;
				_visible = false;
			}

			return;
		}

		var pos = mop.BlockPos;
		var blockId = level.GetBlockId(pos);
		if (blockId == 0)
		{
			if (_visible)
			{
				_outlineMesh.Visible = false;
				_visible = false;
			}

			return;
		}

		var meta = level.GetMetadata(pos);
		var block = Blocks.GetPreset(blockId);
		var aabb = block.GetCube(meta);

		var minX = (float)(aabb.X0 - Expand);
		var minY = (float)(aabb.Y0 - Expand);
		var minZ = (float)(aabb.Z0 - Expand);
		var maxX = (float)(aabb.X1 + Expand);
		var maxY = (float)(aabb.Y1 + Expand);
		var maxZ = (float)(aabb.Z1 + Expand);

		var needRebuild = !_visible
			|| !pos.Equals(_lastPos)
			|| Mathf.Abs((float)(minX - _lastMinX)) > 0.0001f
			|| Mathf.Abs((float)(minY - _lastMinY)) > 0.0001f
			|| Mathf.Abs((float)(minZ - _lastMinZ)) > 0.0001f
			|| Mathf.Abs((float)(maxX - _lastMaxX)) > 0.0001f
			|| Mathf.Abs((float)(maxY - _lastMaxY)) > 0.0001f
			|| Mathf.Abs((float)(maxZ - _lastMaxZ)) > 0.0001f;

		if (needRebuild)
		{
			BuildWireframe(minX, minY, minZ, maxX, maxY, maxZ);
			_lastPos = pos;
			_lastMinX = minX;
			_lastMinY = minY;
			_lastMinZ = minZ;
			_lastMaxX = maxX;
			_lastMaxY = maxY;
			_lastMaxZ = maxZ;
		}

		_outlineMesh.Position = new Vector3(pos.X, pos.Y, pos.Z);

		if (!_visible)
		{
			_outlineMesh.Visible = true;
			_visible = true;
		}
	}

	private void BuildWireframe(float x0, float y0, float z0, float x1, float y1, float z1)
	{
		_immediateMesh.ClearSurfaces();
		_immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, _material);

		AddLine(x0, y0, z0, x1, y0, z0);
		AddLine(x1, y0, z0, x1, y0, z1);
		AddLine(x1, y0, z1, x0, y0, z1);
		AddLine(x0, y0, z1, x0, y0, z0);

		AddLine(x0, y1, z0, x1, y1, z0);
		AddLine(x1, y1, z0, x1, y1, z1);
		AddLine(x1, y1, z1, x0, y1, z1);
		AddLine(x0, y1, z1, x0, y1, z0);

		AddLine(x0, y0, z0, x0, y1, z0);
		AddLine(x1, y0, z0, x1, y1, z0);
		AddLine(x1, y0, z1, x1, y1, z1);
		AddLine(x0, y0, z1, x0, y1, z1);

		_immediateMesh.SurfaceEnd();
	}

	private void AddLine(float x0, float y0, float z0, float x1, float y1, float z1)
	{
		_immediateMesh.SurfaceSetColor(Colors.Black);
		_immediateMesh.SurfaceAddVertex(new Vector3(x0, y0, z0));
		_immediateMesh.SurfaceSetColor(Colors.Black);
		_immediateMesh.SurfaceAddVertex(new Vector3(x1, y1, z1));
	}

	public void Free()
	{
		_outlineMesh.QueueFree();
	}
}
