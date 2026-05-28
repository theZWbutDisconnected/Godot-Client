using System;
using System.Collections.Generic;
using TestClient.Source.World;
using TestClient.Source.World.Entities;
using TestClient.Source.World.Tile;

namespace TestClient.Source.Physics;

public static class Raycast
{
    
    public static MoveObject RayTraceBlocks(
        double startX, double startY, double startZ,
        double endX,   double endY,   double endZ,
        Level level)
    {
        return DdaTrace(startX, startY, startZ, endX, endY, endZ, level);
    }

    public static MoveObject RayTraceEntities(
        double startX, double startY, double startZ,
        double endX,   double endY,   double endZ,
        Level level, Entity exclude = null, double expand = 0.1)
    {
        return EntityTrace(startX, startY, startZ, endX, endY, endZ, level, exclude, expand);
    }

    public static MoveObject RayTrace(
        double startX, double startY, double startZ,
        double endX,   double endY,   double endZ,
        Level level, Entity exclude = null, double entityExpand = 0.1)
    {
        var blockHit  = DdaTrace(startX, startY, startZ, endX, endY, endZ, level);
        var entityHit = EntityTrace(startX, startY, startZ, endX, endY, endZ,
            level, exclude, entityExpand);

        if (blockHit.Type  == MoveObjectType.Miss) return entityHit;
        if (entityHit.Type == MoveObjectType.Miss) return blockHit;

        double blockDistSq  = blockHit.DistanceSq(startX, startY, startZ);
        double entityDistSq = entityHit.DistanceSq(startX, startY, startZ);

        return blockDistSq <= entityDistSq ? blockHit : entityHit;
    }

    private static MoveObject DdaTrace(
        double startX, double startY, double startZ,
        double endX,   double endY,   double endZ,
        Level level)
    {
        double dx = endX - startX;
        double dy = endY - startY;
        double dz = endZ - startZ;

        int stepX = Math.Sign(dx);
        int stepY = Math.Sign(dy);
        int stepZ = Math.Sign(dz);

        if (stepX == 0) stepX = 1;
        if (stepY == 0) stepY = 1;
        if (stepZ == 0) stepZ = 1;

        double tDeltaX = (dx != 0.0) ? Math.Abs(1.0 / dx) : double.PositiveInfinity;
        double tDeltaY = (dy != 0.0) ? Math.Abs(1.0 / dy) : double.PositiveInfinity;
        double tDeltaZ = (dz != 0.0) ? Math.Abs(1.0 / dz) : double.PositiveInfinity;

        int curX = Mth.DoubleToFloorInt(startX);
        int curY = Mth.DoubleToFloorInt(startY);
        int curZ = Mth.DoubleToFloorInt(startZ);

        int endXBlock = Mth.DoubleToFloorInt(endX);
        int endYBlock = Mth.DoubleToFloorInt(endY);
        int endZBlock = Mth.DoubleToFloorInt(endZ);

        double tMaxX = (stepX > 0)
            ? (curX + 1.0 - startX) * tDeltaX
            : (startX - curX)       * tDeltaX;
        double tMaxY = (stepY > 0)
            ? (curY + 1.0 - startY) * tDeltaY
            : (startY - curY)       * tDeltaY;
        double tMaxZ = (stepZ > 0)
            ? (curZ + 1.0 - startZ) * tDeltaZ
            : (startZ - curZ)       * tDeltaZ;

        double invDx = (dx != 0.0) ? 1.0 / dx : 0.0;
        double invDy = (dy != 0.0) ? 1.0 / dy : 0.0;
        double invDz = (dz != 0.0) ? 1.0 / dz : 0.0;

        for (int i = 0; i < 256; i++)
        {
            var hit = CheckBlockHit(startX, startY, startZ,
                dx, dy, dz, invDx, invDy, invDz,
                curX, curY, curZ, level);
            if (hit != null)
                return hit;

            if (curX == endXBlock && curY == endYBlock && curZ == endZBlock)
                break;

            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ) { curX += stepX; tMaxX += tDeltaX; }
                else               { curZ += stepZ; tMaxZ += tDeltaZ; }
            }
            else
            {
                if (tMaxY < tMaxZ) { curY += stepY; tMaxY += tDeltaY; }
                else               { curZ += stepZ; tMaxZ += tDeltaZ; }
            }
        }

        return new MoveObject();
    }

    private static MoveObject CheckBlockHit(
        double startX, double startY, double startZ,
        double dx, double dy, double dz,
        double invDx, double invDy, double invDz,
        int bx, int by, int bz, Level level)
    {
        int blockId = level.GetBlockId(new BlockPos(bx, by, bz));
        if (blockId == 0)
            return null;

        Block block = Blocks.GetPreset(blockId);
        int meta    = level.GetMetadata(new BlockPos(bx, by, bz));
        AABB cube   = block.GetCube(meta);
        if (cube == null)
            return null;

        double x0 = bx + cube.X0,  x1 = bx + cube.X1;
        double y0 = by + cube.Y0,  y1 = by + cube.Y1;
        double z0 = bz + cube.Z0,  z1 = bz + cube.Z1;

        if (!SlabTest(startX, startY, startZ, invDx, invDy, invDz,
                x0, y0, z0, x1, y1, z1,
                out double t, out Direction face))
            return null;

        if (t < 0.0 || t > 1.0)
            return null;

        double hitX = startX + t * dx;
        double hitY = startY + t * dy;
        double hitZ = startZ + t * dz;

        return new MoveObject(MoveObjectType.Block,
            hitX, hitY, hitZ, face, new BlockPos(bx, by, bz), null);
    }

    private static MoveObject EntityTrace(
        double startX, double startY, double startZ,
        double endX,   double endY,   double endZ,
        Level level, Entity exclude, double expand)
    {
        double rMinX = Math.Min(startX, endX);
        double rMinY = Math.Min(startY, endY);
        double rMinZ = Math.Min(startZ, endZ);
        double rMaxX = Math.Max(startX, endX);
        double rMaxY = Math.Max(startY, endY);
        double rMaxZ = Math.Max(startZ, endZ);
        var corridor = new AABB(rMinX, rMinY, rMinZ, rMaxX, rMaxY, rMaxZ)
            .Grow(2.0, 2.0, 2.0);

        double dx = endX - startX;
        double dy = endY - startY;
        double dz = endZ - startZ;

        double invDx = (dx != 0.0) ? 1.0 / dx : 0.0;
        double invDy = (dy != 0.0) ? 1.0 / dy : 0.0;
        double invDz = (dz != 0.0) ? 1.0 / dz : 0.0;

        var candidates = CollectEntityCandidates(level, corridor, exclude);

        MoveObject best   = null;
        double bestDistSq = double.MaxValue;

        foreach (var entity in candidates)
        {
            AABB bb = entity.BoundingBox;
            if (bb == null) continue;

            double x0 = bb.X0 - expand,  x1 = bb.X1 + expand;
            double y0 = bb.Y0 - expand,  y1 = bb.Y1 + expand;
            double z0 = bb.Z0 - expand,  z1 = bb.Z1 + expand;

            if (!SlabTest(startX, startY, startZ, invDx, invDy, invDz,
                    x0, y0, z0, x1, y1, z1,
                    out double t, out Direction face))
                continue;

            if (t < 0.0 || t > 1.0)
                continue;

            double hitX = startX + t * dx;
            double hitY = startY + t * dy;
            double hitZ = startZ + t * dz;

            double distSq = (hitX - startX) * (hitX - startX)
                          + (hitY - startY) * (hitY - startY)
                          + (hitZ - startZ) * (hitZ - startZ);

            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                best = new MoveObject(MoveObjectType.Entity,
                    hitX, hitY, hitZ, face, null, entity);
            }
        }

        return best ?? new MoveObject();
    }

    private static List<Entity> CollectEntityCandidates(
        Level level, AABB corridor, Entity exclude)
    {
        var list = new List<Entity>();
        foreach (var entity in level.Entities)
        {
            if (ReferenceEquals(entity, exclude)) continue;
            if (entity.Removed) continue;
            if (entity.BoundingBox == null) continue;
            if (entity.BoundingBox.Intersects(corridor))
                list.Add(entity);
        }
        return list;
    }

    
    private static bool SlabTest(
        double startX, double startY, double startZ,
        double invDx,  double invDy,  double invDz,
        double x0, double y0, double z0,
        double x1, double y1, double z1,
        out double tNear, out Direction hitFace)
    {
        tNear   = 0.0;
        hitFace = null;

        double tX0 = (invDx != 0.0) ? (x0 - startX) * invDx : double.NegativeInfinity;
        double tX1 = (invDx != 0.0) ? (x1 - startX) * invDx : double.PositiveInfinity;
        if (tX0 > tX1) { double t = tX0; tX0 = tX1; tX1 = t; }

        double tY0 = (invDy != 0.0) ? (y0 - startY) * invDy : double.NegativeInfinity;
        double tY1 = (invDy != 0.0) ? (y1 - startY) * invDy : double.PositiveInfinity;
        if (tY0 > tY1) { double t = tY0; tY0 = tY1; tY1 = t; }

        double tZ0 = (invDz != 0.0) ? (z0 - startZ) * invDz : double.NegativeInfinity;
        double tZ1 = (invDz != 0.0) ? (z1 - startZ) * invDz : double.PositiveInfinity;
        if (tZ0 > tZ1) { double t = tZ0; tZ0 = tZ1; tZ1 = t; }

        double tMin = (tX0 > tY0) ? tX0 : tY0;
        if (tZ0 > tMin) tMin = tZ0;

        double tMax = (tX1 < tY1) ? tX1 : tY1;
        if (tZ1 < tMax) tMax = tZ1;

        if (tMax < 0.0)  return false;   // Behind ray origin
        if (tMin > tMax) return false;   // Misses

        tNear = (tMin >= 0.0) ? tMin : tMax;

        const double eps = 1e-12;
        if (Math.Abs(tMin - tX0) < eps)
            hitFace = (invDx > 0.0) ? Direction.WEST : Direction.EAST;
        else if (Math.Abs(tMin - tY0) < eps)
            hitFace = (invDy > 0.0) ? Direction.DOWN : Direction.UP;
        else
            hitFace = (invDz > 0.0) ? Direction.NORTH : Direction.SOUTH;

        return true;
    }
}
