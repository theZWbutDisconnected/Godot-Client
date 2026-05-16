namespace TestClient.Source.Network.Packet.Server.Play;

public class S40Disconnect : IPacket
{
	public string Reason { get; private set; }

	public void Read(PacketBuffer buf)
	{
		Reason = buf.ReadChatComponent();
	}

	public void Write(PacketBuffer buf)
	{
		/* S2C only, no need to write */
	}
}
