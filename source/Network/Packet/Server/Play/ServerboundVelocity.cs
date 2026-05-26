using System.IO;
using TestClient.Source.Network.NetHandler;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Network.Packet.Server.Play;

public class ServerboundVelocity : IPacket
{
    public int EntityId { get; private set; }
    public int MotionX { get; private set; }
    public int MotionY { get; private set; }
    public int MotionZ { get; private set; }

    public void Read(PacketBuffer buf)
    {
        EntityId = buf.ReadVarInt();
        MotionX = buf.ReadShort();
        MotionY = buf.ReadShort();
        MotionZ = buf.ReadShort();
    }

    public void Write(PacketBuffer stream)
    {
        /* S2C only */
    }
}
