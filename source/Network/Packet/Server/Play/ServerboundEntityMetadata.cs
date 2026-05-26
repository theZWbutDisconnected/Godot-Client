using System.Collections.Generic;
using TestClient.Source.Utility;
using TestClient.Source.World;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Network.Packet.Server.Play;

public class ServerboundEntityMetadata : IPacket
{
    public int EntityId { get; private set; }
    public List<DataWatcher.WatchableObject> WatchableObjects { get; private set; }

    public ServerboundEntityMetadata() { }

    public ServerboundEntityMetadata(int entityId, DataWatcher dataWatcher, bool all)
    {
        EntityId = entityId;
        WatchableObjects = all ? dataWatcher.GetAllWatched() : dataWatcher.GetChanged();
    }

    public void Read(PacketBuffer buf)
    {
        EntityId = buf.ReadVarInt();
        WatchableObjects = DataWatcher.ReadWatchedListFromPacketBuffer(buf);
    }

    public void Write(PacketBuffer buf)
    {
    }

    public Entity GetEntity(Level level)
    {
        return level.GetEntityById(EntityId);
    }
}
