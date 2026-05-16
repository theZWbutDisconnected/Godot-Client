using System;
using System.IO;
using Godot;
using TestClient.Source.Network.Packet.Client.Play;
using TestClient.Source.Network.Packet.Server.Play;

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

    public void HandleKeepAlive(S00KeepAlive packetIn)
    {
        _networkSystem.SendPacket(new C00KeepAlive(packetIn.KeepAliveId));
    }

    public void HandleConfirmTransaction(S32ConfirmTransaction packetIn)
    {
        if (!packetIn.Accepted)
            _networkSystem.SendPacket(new C0FConfirmTransaction(packetIn.WindowId, packetIn.ActionNumber, true));
    }

    public void HandleJoinGame(S01JoinGame packetIn)
    {
        _networkSystem.SendPacket(new C15ClientSettings());
        var brandBuf = new PacketBuffer(new MemoryStream());
        brandBuf.WriteString("vanilla");
        _networkSystem.SendPacket(new C17CustomPayload("MC|Brand", brandBuf));
        Game.Singleton.Player.EntityId = packetIn.EntityId;
        GD.Print($"EntityId={packetIn.EntityId}, GameType={packetIn.GameType}, " +
                 $"Dimension={packetIn.Dimension}, Difficulty={packetIn.Difficulty}, " +
                 $"MaxPlayers={packetIn.MaxPlayers}, WorldType={packetIn.WorldType}");
    }

    public void HandlePlayerPosLook(S08PlayerPosLook packetIn)
    {
        var entityplayer = Game.Singleton.Player;
        var d0 = packetIn.X;
        var d1 = packetIn.Y;
        var d2 = packetIn.Z;
        var f = packetIn.Yaw;
        var f1 = packetIn.Pitch;

        if (packetIn.Flags.Contains(S08PlayerPosLook.EnumFlags.X))
            d0 += entityplayer.PosX;
        else
            entityplayer.XDelta = 0.0D;

        if (packetIn.Flags.Contains(S08PlayerPosLook.EnumFlags.Y))
            d1 += entityplayer.PosY;
        else
            entityplayer.YDelta = 0.0D;

        if (packetIn.Flags.Contains(S08PlayerPosLook.EnumFlags.Z))
            d2 += entityplayer.PosZ;
        else
            entityplayer.ZDelta = 0.0D;

        if (packetIn.Flags.Contains(S08PlayerPosLook.EnumFlags.Pitch)) f1 += entityplayer.RotX;

        if (packetIn.Flags.Contains(S08PlayerPosLook.EnumFlags.Yaw)) f += entityplayer.RotY;

        entityplayer.SetPosAndRot(d0, d1, d2, f, f1);

        _networkSystem.SendPacket(new C06PlayerPosLook(entityplayer.PosX, entityplayer.BoundingBox.Y0,
            entityplayer.PosZ,
            entityplayer.RotY, entityplayer.RotX, false));
        entityplayer.PrevX = entityplayer.PosX;
        entityplayer.PrevY = entityplayer.PosY;
        entityplayer.PrevZ = entityplayer.PosZ;
        GD.Print("Server position set: x - ", entityplayer.PosX, " y - ", entityplayer.PosY, " z - ", entityplayer.PosZ,
            " yaw - ",
            entityplayer.RotY, " pitch - ", entityplayer.RotX);
    }

    public void HandleEntityTeleport(S18EntityTeleport packetIn)
    {
        var entity = Game.Singleton.Level.GetEntityById(packetIn.EntityId);
        if (entity != null)
        {
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
    }

    public void HandleEntityHeadLook(S19EntityHeadLook packetIn)
    {
        var entity = Game.Singleton.Level.GetEntityById(packetIn.EntityId);
        if (entity != null)
        {
            var f = packetIn.Yaw * 360 / 256.0F;
            entity.SetHeadYaw(f);
        }
    }

    public void HandleChunkData(S21ChunkData @in)
    {
        var level = Game.Singleton.Level;
        if (@in.Chunk != null) level.AddChunk(@in.Chunk);
    }

    public void HandleMapChunkBulk(S26MapChunkBulk @in)
    {
        var level = Game.Singleton.Level;
        foreach (var chunk in @in.Chunks) level.AddChunk(chunk);
    }

    public void HandleMultiBlockChange(S22MultiBlockChange @in)
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

    public void HandleBlockChange(S23BlockChange @in)
    {
        var level = Game.Singleton.Level;
        var pos = @in.BlockPos;
        var blockStateId = @in.BlockStateId;

        var blockId = blockStateId >> 4;
        var metadata = blockStateId & 0xF;

        level.SetBlock(pos, blockId, metadata);
        GD.Print("S23 Block changed at (" + pos.X + ", " + pos.Y + ", " + pos.Z + ") to state: " + blockStateId);
    }

    public void HandleDisconnect(S40Disconnect packetIn)
    {
        var reason = packetIn.Reason;
        GD.Print("[Disconnect] Server kicked with reason: " + reason);
        _networkSystem.Disconnect();
    }

    public void Disconnected(string reason)
    {
        GD.PrintErr($"Play connection lost: {reason}");
    }
}