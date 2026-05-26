namespace TestClient.Source.Network.Packet.Server.Play;

public class ServerboundAbilities : IPacket
{
    public bool Invulnerable { get; private set; }
    public bool Flying { get; private set; }
    public bool AllowFlying { get; private set; }
    public bool CreativeMode { get; private set; }
    public float FlySpeed { get; private set; }
    public float WalkSpeed { get; private set; }

    public void Read(PacketBuffer buf)
    {
        var flags = buf.ReadUnsignedByte();
        Invulnerable = (flags & 0x01) != 0;
        Flying       = (flags & 0x02) != 0;
        AllowFlying  = (flags & 0x04) != 0;
        CreativeMode = (flags & 0x08) != 0;
        FlySpeed     = buf.ReadFloat();
        WalkSpeed    = buf.ReadFloat();
    }

    public void Write(PacketBuffer stream)
    {
        // S2C only, no writing needed
    }
}