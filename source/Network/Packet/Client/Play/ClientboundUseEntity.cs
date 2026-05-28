using Godot;
using TestClient.Source.World;
using TestClient.Source.World.Entities;

namespace TestClient.Source.Network.Packet.Client.Play;

public class ClientboundUseEntity : IPacket
{
    public int EntityId { get; private set; }
    public UseEntityAction Action { get; private set; }
    public Vector3? HitVec { get; private set; }

    public ClientboundUseEntity()
    {
    }

    public ClientboundUseEntity(Entity entity, UseEntityAction action)
    {
        EntityId = entity.EntityId;
        Action = action;
    }

    public ClientboundUseEntity(Entity entity, Vector3 hitVec)
        : this(entity, UseEntityAction.InteractAt)
    {
        HitVec = hitVec;
    }

    public void Read(PacketBuffer buf)
    {
        /* C2S only */
    }

    public void Write(PacketBuffer buf)
    {
        buf.WriteVarInt(EntityId);
        buf.WriteVarInt((int)Action);

        if (Action == UseEntityAction.InteractAt && HitVec.HasValue)
        {
            buf.WriteFloat(HitVec.Value.X);
            buf.WriteFloat(HitVec.Value.Y);
            buf.WriteFloat(HitVec.Value.Z);
        }
    }

    public Entity GetEntity(Level worldIn)
    {
        return worldIn.GetEntityById(EntityId);
    }

    public enum UseEntityAction
    {
        Interact,
        Attack,
        InteractAt
    }
}
