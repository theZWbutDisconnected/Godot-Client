namespace TestClient.Source.Network.Packet.Client.Play;

public class C03Player : IPacket
{
    protected bool Moving;
    protected bool OnGround;
    protected float Pitch;
    protected bool Rotating;
    protected double X;
    protected double Y;
    protected float Yaw;
    protected double Z;

    public C03Player()
    {
    }

    public C03Player(bool isOnGround)
    {
        OnGround = isOnGround;
    }

    public virtual void Read(PacketBuffer buf)
    {
        /* C2S only, no need to read */
    }

    public virtual void Write(PacketBuffer buf)
    {
        buf.WriteBoolean(OnGround);
    }
}

public class C04PlayerPosition : C03Player
{
    public C04PlayerPosition(double posX, double posY, double posZ, bool isOnGround)
    {
        X = posX;
        Y = posY;
        Z = posZ;
        OnGround = isOnGround;
        Moving = true;
    }

    public override void Write(PacketBuffer buf)
    {
        buf.WriteDouble(X);
        buf.WriteDouble(Y);
        buf.WriteDouble(Z);
        base.Write(buf);
    }
}

public class C05PlayerLook : C03Player
{
    public C05PlayerLook(float playerYaw, float playerPitch, bool isOnGround)
    {
        Yaw = playerYaw;
        Pitch = playerPitch;
        OnGround = isOnGround;
        Rotating = true;
    }

    public override void Write(PacketBuffer buf)
    {
        buf.WriteFloat(Yaw);
        buf.WriteFloat(Pitch);
        base.Write(buf);
    }
}

public class C06PlayerPosLook : C03Player
{
    public C06PlayerPosLook(double playerX, double playerY, double playerZ,
        float playerYaw, float playerPitch, bool playerIsOnGround)
    {
        X = playerX;
        Y = playerY;
        Z = playerZ;
        Yaw = playerYaw;
        Pitch = playerPitch;
        OnGround = playerIsOnGround;
        Moving = true;
        Rotating = true;
    }

    public override void Write(PacketBuffer buf)
    {
        buf.WriteDouble(X);
        buf.WriteDouble(Y);
        buf.WriteDouble(Z);
        buf.WriteFloat(Yaw);
        buf.WriteFloat(Pitch);
        base.Write(buf);
    }
}