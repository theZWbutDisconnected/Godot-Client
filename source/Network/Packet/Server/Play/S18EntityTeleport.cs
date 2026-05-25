namespace TestClient.Source.Network.Packet.Server.Play;

public class S18EntityTeleport : IPacket
{
    public int EntityId { get; private set; }
    public int PosX { get; private set; }
    public int PosY { get; private set; }
    public int PosZ { get; private set; }
    public byte Yaw { get; private set; }
    public byte Pitch { get; private set; }
    public bool OnGround { get; private set; }

    public void Read(PacketBuffer buf)
    {
        EntityId = buf.ReadVarInt();
        PosX = buf.ReadInt();
        PosY = buf.ReadInt();
        PosZ = buf.ReadInt();
        Yaw = buf.ReadUnsignedByte();
        Pitch = buf.ReadUnsignedByte();
        OnGround = buf.ReadBoolean();
    }

    public void Write(PacketBuffer stream)
    {
        /* S2C only */
    }
}