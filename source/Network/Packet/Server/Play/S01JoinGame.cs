namespace TestClient.Source.Network.Packet.Server.Play;

public class S01JoinGame : IPacket
{
	public int EntityId { get; private set; }
	public int GameType { get; private set; }
	public bool HardcoreMode { get; private set; }
	public int Dimension { get; private set; }
	public int Difficulty { get; private set; }
	public int MaxPlayers { get; private set; }
	public string WorldType { get; private set; } = "default";
	public bool ReducedDebugInfo { get; private set; }

	public void Read(PacketBuffer buf)
	{
		EntityId = buf.ReadInt();
		var raw = buf.ReadByte() & 0xFF;
		HardcoreMode = (raw & 8) == 8;
		GameType = raw & ~8;
		Dimension = buf.ReadSignedByte();
		Difficulty = buf.ReadByte() & 0xFF;
		MaxPlayers = buf.ReadByte() & 0xFF;
		WorldType = buf.ReadString(16) ?? "default";
		ReducedDebugInfo = buf.ReadBoolean();
	}

	public void Write(PacketBuffer buf)
	{
		/* S2C only, no need to write */
	}
}
