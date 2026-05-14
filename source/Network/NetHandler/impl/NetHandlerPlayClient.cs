using System.IO;
using Godot;
using TestClient.Source.Network.Packet.Client.Play;
using TestClient.Source.Network.Packet.Server.Play;
using TestClient.Source.World.Entity;

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
        var brandBuf = new PacketBuffer(new MemoryStream());
        brandBuf.WriteString("vanilla");
        _networkSystem.SendPacket(new C17CustomPayload("MC|Brand", brandBuf));
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
            d0 += entityplayer.X;
        else
            entityplayer.XDelta = 0.0D;

        if (packetIn.Flags.Contains(S08PlayerPosLook.EnumFlags.Y))
            d1 += entityplayer.Y;
        else
            entityplayer.YDelta = 0.0D;

        if (packetIn.Flags.Contains(S08PlayerPosLook.EnumFlags.Z))
            d2 += entityplayer.Z;
        else
            entityplayer.ZDelta = 0.0D;

        if (packetIn.Flags.Contains(S08PlayerPosLook.EnumFlags.Pitch)) f1 += entityplayer.Pitch;

        if (packetIn.Flags.Contains(S08PlayerPosLook.EnumFlags.Yaw)) f += entityplayer.Yaw;

        entityplayer.SetPosAndRot((float)d0, (float)d1, (float)d2, f, f1);
        _networkSystem.SendPacket(new C06PlayerPosLook(entityplayer.X, entityplayer.BoundingBox.Y0, entityplayer.Z,
            entityplayer.Yaw, entityplayer.Pitch, false));
        GD.Print("Server position set: x -", entityplayer.X, "y -", entityplayer.Y, "z -", entityplayer.Z, "yaw -", entityplayer.Yaw, "pitch -", entityplayer.Pitch);
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