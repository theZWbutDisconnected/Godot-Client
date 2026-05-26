using System;
using System.Collections.Generic;
using TestClient.Source.Utility;

namespace TestClient.Source.Network.Packet.Server.Play;

public class ServerboundSpawnPlayer : IPacket
{
    public int EntityId { get; private set; }
    public Guid PlayerId { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Z { get; private set; }
    public sbyte Yaw { get; private set; }
    public sbyte Pitch { get; private set; }
    public int CurrentItem { get; private set; }
    public List<DataWatcher.WatchableObject> Watcher { get; private set; }

    public void Read(PacketBuffer buf)
    {
        EntityId = buf.ReadVarInt();
        PlayerId = buf.ReadUuid();
        X = buf.ReadInt();
        Y = buf.ReadInt();
        Z = buf.ReadInt();
        Yaw = buf.ReadByte();
        Pitch = buf.ReadByte();
        CurrentItem = buf.ReadShort();
        Watcher = DataWatcher.ReadWatchedListFromPacketBuffer(buf);
    }

    public void Write(PacketBuffer buf)
    {
        /* S2C only, no need to write */
    }
}
