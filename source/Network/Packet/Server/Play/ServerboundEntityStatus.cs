using TestClient.Source.World;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Network.Packet.Server.Play;

public class ServerboundEntityStatus : IPacket
{
    public int EntityId { get; private set; }
    public bool OnGround { get; private set; }
    public sbyte PosX { get; private set; }
    public sbyte PosY { get; private set; }
    public sbyte PosZ { get; private set; }
    public sbyte Yaw { get; private set; }
    public sbyte Pitch { get; private set; }
    public bool LookChange { get; private set; }
    
    
    public virtual void Read(PacketBuffer buf)
    {
        EntityId = buf.ReadVarInt();
    }

    public void Write(PacketBuffer stream)
    {
        /* S2C only */
    }

    public override string ToString()
    {
        return "Entity_" + base.ToString();
    }

    public Entity GetEntity(Level worldIn)
    {
        return worldIn.GetEntityById(EntityId);
    }

    public class ServerboundEntityMove : ServerboundEntityStatus
    {
        public override void Read(PacketBuffer buf)
        {
            base.Read(buf);
            PosX = buf.ReadByte();
            PosY = buf.ReadByte();
            PosZ = buf.ReadByte();
            OnGround = buf.ReadBoolean();
        }
    }

    public class ServerboundEntityLook : ServerboundEntityStatus
    {
        public ServerboundEntityLook()
        {
            LookChange = true;
        }

        public override void Read(PacketBuffer buf)
        {
            base.Read(buf);
            Yaw = buf.ReadByte();
            Pitch = buf.ReadByte();
            OnGround = buf.ReadBoolean();
        }
    }

    public class ServerboundEntityLookMove : ServerboundEntityStatus
    {
        public ServerboundEntityLookMove()
        {
            LookChange = true;
        }

        public override void Read(PacketBuffer buf)
        {
            base.Read(buf);
            PosX = buf.ReadByte();
            PosY = buf.ReadByte();
            PosZ = buf.ReadByte();
            Yaw = buf.ReadByte();
            Pitch = buf.ReadByte();
            OnGround = buf.ReadBoolean();
        }
    }
}