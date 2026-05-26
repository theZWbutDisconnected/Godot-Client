using System.Collections.Generic;
using TestClient.Source.Utility;

namespace TestClient.Source.Network.Packet.Server.Play;

public class ServerboundSpawnMob : IPacket
{
    public int EntityId { get; private set; }
    public int Type { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Z { get; private set; }
    public sbyte Yaw { get; private set; }
    public sbyte Pitch { get; private set; }
    public sbyte HeadYaw { get; private set; }
    public short VelocityX { get; private set; }
    public short VelocityY { get; private set; }
    public short VelocityZ { get; private set; }
    public List<DataWatcher.WatchableObject> Watcher { get; private set; }

    public void Read(PacketBuffer buf)
    {
        EntityId = buf.ReadVarInt();
        Type = buf.ReadUnsignedByte();
        X = buf.ReadInt();
        Y = buf.ReadInt();
        Z = buf.ReadInt();
        Yaw = buf.ReadByte();
        Pitch = buf.ReadByte();
        HeadYaw = buf.ReadByte();
        VelocityX = buf.ReadShort();
        VelocityY = buf.ReadShort();
        VelocityZ = buf.ReadShort();
        Watcher = DataWatcher.ReadWatchedListFromPacketBuffer(buf);
    }

    public void Write(PacketBuffer buf)
    {
        /* S2C only, no need to write */
    }
}
