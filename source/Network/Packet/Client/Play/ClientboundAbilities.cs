namespace TestClient.Source.Network.Packet.Client.Play;


public class ClientboundAbilities : IPacket
{
    public bool Invulnerable { get; set; }
    public bool Flying { get; set; }
    public bool AllowFlying { get; set; }
    public bool CreativeMode { get; set; }
    public float FlySpeed { get; set; }
    public float WalkSpeed { get; set; }

    public ClientboundAbilities() { }

    public ClientboundAbilities(Capabilities capabilities)
    {
        Invulnerable = capabilities.DisableDamage;
        Flying = capabilities.IsFlying;
        AllowFlying = capabilities.AllowFlying;
        CreativeMode = capabilities.IsCreativeMode;
        FlySpeed = capabilities.FlySpeed;
        WalkSpeed = capabilities.WalkSpeed;
    }

    public void Read(PacketBuffer buf)
    {
        // C2S only, no reading needed
    }

    public void Write(PacketBuffer stream)
    {
        byte flags = 0;
        if (Invulnerable) flags |= 0x01;
        if (Flying)       flags |= 0x02;
        if (AllowFlying)  flags |= 0x04;
        if (CreativeMode) flags |= 0x08;

        stream.WriteByte(flags);
        stream.WriteFloat(FlySpeed);
        stream.WriteFloat(WalkSpeed);
    }
}