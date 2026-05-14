namespace TestClient.Source.Network.Packet.Server.Play;

public class S00KeepAlive : IPacket
{
	public int KeepAliveId { get; private set; }

	public void Read(PacketBuffer buf)
	{
		KeepAliveId = buf.ReadVarInt();
	}

	public void Write(PacketBuffer buf)
	{
		/* S2C only, no need to write */
	}
}
