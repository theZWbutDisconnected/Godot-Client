using System.Collections.Generic;

namespace TestClient.Source.Network.Packet.Server.Play;

public class ServerboundDestroyEntities : IPacket
{
    public List<int> EntityIds { get; private set; }

    public void Read(PacketBuffer buf)
    {
        var count = buf.ReadVarInt();
        EntityIds = new List<int>(count);
        for (var i = 0; i < count; i++)
        {
            EntityIds.Add(buf.ReadVarInt());
        }
    }

    public void Write(PacketBuffer buf)
    {
        /* S2C only, no need to write */
    }
}
