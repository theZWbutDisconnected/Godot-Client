using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using TestClient.Source.Network.Packet.Client.Play;
using TestClient.Source.Network.Packet.Server.Play;
using TestClient.Source.Utility;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Network.NetHandler.impl;

public class NetHandlerPlayClient : INetHandlerPlayClient
{
    private readonly NetworkSystem _networkSystem;
    private readonly string _username;

    public NetHandlerPlayClient(NetworkSystem networkSystem)
    {
        _networkSystem = networkSystem;
        _username = networkSystem.Username;
    }

    public void HandleKeepAlive(ServerboundKeepAlive packetIn)
    {
        _networkSystem.SendPacket(new ClientboundKeepAlive(packetIn.KeepAliveId));
    }

    public void HandleConfirmTransaction(ServerboundConfirmTransaction packetIn)
    {
        // if (!packetIn.Accepted)
        //     _networkSystem.SendPacket(new C0FConfirmTransaction(packetIn.WindowId, packetIn.ActionNumber, true));
    }

    public void HandleJoinGame(ServerboundJoinGame packetIn)
    {
        _networkSystem.SendPacket(new ClientboundSettings());
        var brandBuf = new PacketBuffer(new MemoryStream());
        brandBuf.WriteString("vanilla");
        _networkSystem.SendPacket(new ClientboundCustomPayload("MC|Brand", brandBuf));
        Game.Singleton.Player.EntityId = packetIn.EntityId;
        GD.Print($"EntityId={packetIn.EntityId}, GameType={packetIn.GameType}, " +
                 $"Dimension={packetIn.Dimension}, Difficulty={packetIn.Difficulty}, " +
                 $"MaxPlayers={packetIn.MaxPlayers}, WorldType={packetIn.WorldType}");
    }
    
    public void HandleAnimation(ServerboundAnimation packetIn)
    {
        Entity entity = Game.Singleton.Level.GetEntityById(packetIn.EntityId);
        GD.Print("S0B Received ", packetIn.Type, " ", packetIn.EntityId, " ", entity != null);

        if (entity != null)
        {
            if (packetIn.Type == 0)
            {
                entity.Swing();
            }
            else if (packetIn.Type == 1)
            {
                entity.HurtAnimation();
            }
        }
    }

    public void HandleSpawnMob(ServerboundSpawnMob packetIn)
    {
        var d0 = packetIn.X / 32.0D;
        var d1 = packetIn.Y / 32.0D;
        var d2 = packetIn.Z / 32.0D;
        var f = packetIn.Yaw * 360 / 256.0F;
        var f1 = packetIn.Pitch * 360 / 256.0F;
        var entitylivingbase = EntityList.CreateEntityByID(packetIn.Type, Game.Singleton.Level);
        if (entitylivingbase == null)
        {
            GD.Print("Unhandled entity id: ", packetIn.Type);
            return;
        }
        entitylivingbase.ServerX = packetIn.X;
        entitylivingbase.ServerY = packetIn.Y;
        entitylivingbase.ServerZ = packetIn.Z;
        entitylivingbase.RotYBody = entitylivingbase.RotYHead = packetIn.HeadYaw * 360 / 256.0F;

        entitylivingbase.EntityId = packetIn.EntityId;
        entitylivingbase.SetPosAndRot(d0, d1, d2, f, f1);
        entitylivingbase.XDelta = packetIn.VelocityX / 8000.0F;
        entitylivingbase.YDelta = packetIn.VelocityY / 8000.0F;
        entitylivingbase.ZDelta = packetIn.VelocityZ / 8000.0F;
        Game.Singleton.Level.AddEntity(packetIn.EntityId, entitylivingbase);
        List<DataWatcher.WatchableObject> list = packetIn.Watcher;
        if (list != null) entitylivingbase.DataWatcher.UpdateWatchedObjectsFromList(list);
    }

    public void HandlePlayerPosLook(ServerboundTeleport packetIn)
    {
        var entityplayer = Game.Singleton.Player;
        var d0 = packetIn.X;
        var d1 = packetIn.Y;
        var d2 = packetIn.Z;
        var f = packetIn.Yaw;
        var f1 = packetIn.Pitch;

        if (packetIn.Flags.Contains(ServerboundTeleport.EnumFlags.X))
            d0 += entityplayer.PosX;
        else
            entityplayer.XDelta = 0.0D;

        if (packetIn.Flags.Contains(ServerboundTeleport.EnumFlags.Y))
            d1 += entityplayer.PosY;
        else
            entityplayer.YDelta = 0.0D;

        if (packetIn.Flags.Contains(ServerboundTeleport.EnumFlags.Z))
            d2 += entityplayer.PosZ;
        else
            entityplayer.ZDelta = 0.0D;

        if (packetIn.Flags.Contains(ServerboundTeleport.EnumFlags.Pitch)) f1 += entityplayer.RotX;

        if (packetIn.Flags.Contains(ServerboundTeleport.EnumFlags.Yaw)) f += entityplayer.RotY;

        entityplayer.SetPosAndRot(d0, d1, d2, f, f1);

        _networkSystem.SendPacket(new ClientboundPlayerPosLook(entityplayer.PosX, entityplayer.BoundingBox.Y0,
            entityplayer.PosZ,
            entityplayer.RotY, entityplayer.RotX, false));
        GD.Print("Server position set: x - ", entityplayer.PosX, " y - ", entityplayer.PosY, " z - ", entityplayer.PosZ,
            " yaw - ",
            entityplayer.RotY, " pitch - ", entityplayer.RotX);
    }

    public void HandleEntityVelocity(ServerboundVelocity packetIn)
    {
        var entity = Game.Singleton.Level.GetEntityById(packetIn.EntityId);
        if (entity == null) return;
        entity.SetVelocity(packetIn.MotionX / 8000.0D, packetIn.MotionY / 8000.0D, packetIn.MotionZ / 8000.0D);
    }

    public void HandleEntityTeleport(ServerboundEntityTeleport packetIn)
    {
        var entity = Game.Singleton.Level.GetEntityById(packetIn.EntityId);
        if (entity == null) return;
        entity.ServerX = packetIn.PosX;
        entity.ServerY = packetIn.PosY;
        entity.ServerZ = packetIn.PosZ;
        var d0 = entity.ServerX / 32.0F;
        var d1 = entity.ServerY / 32.0F;
        var d2 = entity.ServerZ / 32.0F;
        var f = packetIn.Yaw * 360 / 256.0F;
        var f1 = packetIn.Pitch * 360 / 256.0F;

        if (Math.Abs(entity.PosX - d0) < 0.03125D && Math.Abs(entity.PosY - d1) < 0.015625D &&
            Math.Abs(entity.PosZ - d2) < 0.03125D)
            entity.SetPosAndRot2(entity.PosX, entity.PosY, entity.PosZ, f, f1, 3, true);
        else
            entity.SetPosAndRot2(d0, d1, d2, f, f1, 3, true);

        entity.OnGround = packetIn.OnGround;
    }

    public void HandleEntityMovement(ServerboundEntityStatus packetIn)
    {
        var entity = packetIn.GetEntity(Game.Singleton.Level);
        if (entity == null) return;
        entity.ServerX += packetIn.PosX;
        entity.ServerY += packetIn.PosY;
        entity.ServerZ += packetIn.PosZ;
        var d0 = entity.ServerX / 32.0D;
        var d1 = entity.ServerY / 32.0D;
        var d2 = entity.ServerZ / 32.0D;
        var f = packetIn.LookChange ? packetIn.Yaw * 360 / 256.0F : entity.RotY;
        var f1 = packetIn.LookChange ? packetIn.Pitch * 360 / 256.0F : entity.RotX;
        entity.SetPosAndRot2(d0, d1, d2, f, f1, 3, false);
        entity.OnGround = packetIn.OnGround;
    }

    public void HandleEntityHeadLook(ServerboundHeadLook packetIn)
    {
        var entity = Game.Singleton.Level.GetEntityById(packetIn.EntityId);
        if (entity != null)
        {
            var f = packetIn.Yaw * 360 / 256.0F;
            entity.SetHeadYaw(f);
        }
    }

    public void HandleChunkData(ServerboundChunkData @in)
    {
        var level = Game.Singleton.Level;
        if (@in.Chunk != null) level.AddChunk(@in.Chunk);
    }

    public void HandleMapChunkBulk(ServerboundMapChunkBulk @in)
    {
        var level = Game.Singleton.Level;
        foreach (var chunk in @in.Chunks) level.AddChunk(chunk);
    }

    public void HandleMultiBlockChange(ServerboundMultiBlockChange @in)
    {
        var level = Game.Singleton.Level;
        var chunkCoord = @in.ChunkPos;

        foreach (var updateData in @in.ChangedBlocks)
        {
            var pos = updateData.GetPos(chunkCoord);
            var blockStateId = updateData.BlockStateId;

            var blockId = blockStateId >> 4;
            var metadata = blockStateId & 0xF;

            level.SetBlock(pos, blockId, metadata);
            GD.Print(
                "S22 MultiBlock changed at (" + pos.X + ", " + pos.Y + ", " + pos.Z + ") to state: " + blockStateId);
        }
    }

    public void HandleBlockChange(ServerboundBlockChange @in)
    {
        var level = Game.Singleton.Level;
        var pos = @in.BlockPos;
        var blockStateId = @in.BlockStateId;

        var blockId = blockStateId >> 4;
        var metadata = blockStateId & 0xF;

        level.SetBlock(pos, blockId, metadata);
        GD.Print("S23 Block changed at (" + pos.X + ", " + pos.Y + ", " + pos.Z + ") to state: " + blockStateId);
    }

    public void HandlePlayerAbilities(ServerboundAbilities packetIn)
    {
        var player = Game.Singleton.Player;
        player.Capabilities.IsFlying = packetIn.Flying;
        player.Capabilities.IsCreativeMode = packetIn.CreativeMode;
        player.Capabilities.DisableDamage = packetIn.Invulnerable;
        player.Capabilities.AllowFlying = packetIn.AllowFlying;
        player.Capabilities.FlySpeed = packetIn.FlySpeed;
        player.Capabilities.WalkSpeed = packetIn.WalkSpeed;
    }

    public void HandleDisconnect(ServerboundDisconnect packetIn)
    {
        var reason = packetIn.Reason;
        GD.Print("[Disconnect] Server kicked with reason: " + reason);
        _networkSystem.Disconnect();
    }

    public void HandleSpawnPlayer(ServerboundSpawnPlayer packetIn)
    {
        var d0 = packetIn.X / 32.0D;
        var d1 = packetIn.Y / 32.0D;
        var d2 = packetIn.Z / 32.0D;
        var f = packetIn.Yaw * 360 / 256.0F;
        var f1 = packetIn.Pitch * 360 / 256.0F;

        var player = new ServerPlayer(Game.Singleton.Level);
        player.ServerX = packetIn.X;
        player.ServerY = packetIn.Y;
        player.ServerZ = packetIn.Z;
        player.SetPosAndRot(d0, d1, d2, f, f1);

        Game.Singleton.Level.AddEntity(packetIn.EntityId, player);

        List<DataWatcher.WatchableObject> list = packetIn.Watcher;
        if (list != null) player.DataWatcher.UpdateWatchedObjectsFromList(list);
    }

    public void Disconnected(string reason)
    {
        GD.PrintErr($"Play connection lost: {reason}");
    }
}