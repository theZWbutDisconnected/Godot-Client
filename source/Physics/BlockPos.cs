using System;
using System.Collections.Generic;

namespace TestClient.Source.Physics;

public class BlockPos : Vec3i
{
    public static readonly BlockPos Origin = new(0, 0, 0);

    private static readonly int NumXBits = 1 + Mth.CalculateLogBaseTwo(Mth.RoundUpToPowerOfTwo(30000000));
    private static readonly int NumZBits = NumXBits;
    private static readonly int NumYBits = 64 - NumXBits - NumZBits;
    private static readonly int YShift = NumZBits;
    private static readonly int XShift = YShift + NumYBits;
    private static readonly long XMask = (1L << NumXBits) - 1L;
    private static readonly long YMask = (1L << NumYBits) - 1L;
    private static readonly long ZMask = (1L << NumZBits) - 1L;

    public BlockPos(int x, int y, int z) : base(x, y, z)
    {
    }

    public BlockPos(double x, double y, double z) : base(x, y, z)
    {
    }

    public BlockPos(Vec3i source) : base(source.X, source.Y, source.Z)
    {
    }

    public BlockPos Add(int x, int y, int z)
    {
        return (x | y | z) == 0 ? this : new BlockPos(X + x, Y + y, Z + z);
    }

    public BlockPos Add(Vec3i vec)
    {
        return Add(vec.X, vec.Y, vec.Z);
    }

    public BlockPos Subtract(Vec3i vec)
    {
        return (vec.X | vec.Y | vec.Z) == 0 ? this : new BlockPos(X - vec.X, Y - vec.Y, Z - vec.Z);
    }

    public BlockPos Up()
    {
        return Up(1);
    }

    public BlockPos Up(int n)
    {
        return Offset(Direction.UP, n);
    }

    public BlockPos Down()
    {
        return Down(1);
    }

    public BlockPos Down(int n)
    {
        return Offset(Direction.DOWN, n);
    }

    public BlockPos North()
    {
        return North(1);
    }

    public BlockPos North(int n)
    {
        return Offset(Direction.NORTH, n);
    }

    public BlockPos South()
    {
        return South(1);
    }

    public BlockPos South(int n)
    {
        return Offset(Direction.SOUTH, n);
    }

    public BlockPos West()
    {
        return West(1);
    }

    public BlockPos West(int n)
    {
        return Offset(Direction.WEST, n);
    }

    public BlockPos East()
    {
        return East(1);
    }

    public BlockPos East(int n)
    {
        return Offset(Direction.EAST, n);
    }

    public BlockPos Offset(Direction facing)
    {
        return Offset(facing, 1);
    }

    public BlockPos Offset(Direction facing, int n)
    {
        return n == 0 ? this : new BlockPos(X + facing.OffsetX * n, Y + facing.OffsetY * n, Z + facing.OffsetZ * n);
    }

    public BlockPos CrossProduct(Vec3i vec)
    {
        return new BlockPos(
            Y * vec.Z - Z * vec.Y,
            Z * vec.X - X * vec.Z,
            X * vec.Y - Y * vec.X
        );
    }

    public long ToLong()
    {
        return (X & XMask) << XShift |
               (Y & YMask) << YShift |
               (Z & ZMask);
    }

    public static BlockPos FromLong(long serialized)
    {
        var x = (int)((serialized >> XShift) & XMask);
        var y = (int)((serialized >> YShift) & YMask);
        var z = (int)(serialized & ZMask);
        return new BlockPos(x, y, z);
    }

    public static IEnumerable<BlockPos> GetAllInBox(BlockPos from, BlockPos to)
    {
        var min = new BlockPos(Math.Min(from.X, to.X), Math.Min(from.Y, to.Y), Math.Min(from.Z, to.Z));
        var max = new BlockPos(Math.Max(from.X, to.X), Math.Max(from.Y, to.Y), Math.Max(from.Z, to.Z));

        for (var y = min.Y; y <= max.Y; y++)
        for (var z = min.Z; z <= max.Z; z++)
        for (var x = min.X; x <= max.X; x++)
            yield return new BlockPos(x, y, z);
    }

    public static IEnumerable<MutableBlockPos> GetAllInBoxMutable(BlockPos from, BlockPos to)
    {
        var min = new BlockPos(Math.Min(from.X, to.X), Math.Min(from.Y, to.Y), Math.Min(from.Z, to.Z));
        var max = new BlockPos(Math.Max(from.X, to.X), Math.Max(from.Y, to.Y), Math.Max(from.Z, to.Z));

        var mutable = new MutableBlockPos();
        for (var y = min.Y; y <= max.Y; y++)
        for (var z = min.Z; z <= max.Z; z++)
        for (var x = min.X; x <= max.X; x++)
        {
            mutable.Set(x, y, z);
            yield return mutable;
        }
    }

    public static BlockPos operator +(BlockPos a, Vec3i b)
    {
        return a.Add(b);
    }

    public static BlockPos operator -(BlockPos a, Vec3i b)
    {
        return a.Subtract(b);
    }

    public static bool operator ==(BlockPos left, BlockPos right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BlockPos left, BlockPos right)
    {
        return !Equals(left, right);
    }

    public override bool Equals(object obj)
    {
        return obj is BlockPos other && X == other.X && Y == other.Y && Z == other.Z;
    }

    public override int GetHashCode()
    {
        return (X, Y, Z).GetHashCode();
    }

    public sealed class MutableBlockPos : BlockPos
    {
        public MutableBlockPos() : this(0, 0, 0)
        {
        }

        public MutableBlockPos(int x, int y, int z) : base(0, 0, 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public new int X { get; private set; }

        public new int Y { get; private set; }

        public new int Z { get; private set; }

        public MutableBlockPos Set(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
            return this;
        }

        public new MutableBlockPos Add(int x, int y, int z)
        {
            X += x;
            Y += y;
            Z += z;
            return this;
        }

        public new MutableBlockPos Offset(Direction facing, int n)
        {
            if (n != 0)
            {
                X += facing.OffsetX * n;
                Y += facing.OffsetY * n;
                Z += facing.OffsetZ * n;
            }

            return this;
        }
    }
}