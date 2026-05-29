using TestClient.Source.World;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Network.Packet.Server.Play;

public class ServerboundEntityAnimation : IPacket
{
    public int EntityId { get; private set; }
    public sbyte OpCode { get; private set; }

    public void Read(PacketBuffer buf)
    {
        EntityId = buf.ReadInt();
        OpCode = buf.ReadByte();
    }

    public void Write(PacketBuffer stream)
    {
        /* S2C only */
    }

    public Entity GetEntity(Level worldIn)
    {
        return worldIn.GetEntityById(EntityId);
    }
}
