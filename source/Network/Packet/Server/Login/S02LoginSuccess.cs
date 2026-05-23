namespace TestClient.Source.Network.Packet.Server.Login;

public class S02LoginSuccess : IPacket
{
	public string Uuid { get; private set; }
	public string Username { get; private set; }

	public void Read(PacketBuffer buf)
	{
		Uuid = buf.ReadString(36);
		Username = buf.ReadString(16);
	}

	public void Write(PacketBuffer buf)
	{
		/* S2C only, no need to write */
	}
}
