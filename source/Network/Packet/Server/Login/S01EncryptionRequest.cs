using System;

namespace TestClient.Source.Network.Packet.Server.Login;

public class S01EncryptionRequest : IPacket
{
	public string ServerId { get; private set; }
	public byte[] PublicKey { get; private set; } = Array.Empty<byte>();
	public byte[] VerifyToken { get; private set; } = Array.Empty<byte>();

	public void Read(PacketBuffer buf)
	{
		ServerId = buf.ReadString(20);
		PublicKey = buf.ReadByteArray();
		VerifyToken = buf.ReadByteArray();
	}

	public void Write(PacketBuffer buf)
	{
		/* S2C only, no need to write */
	}
}
