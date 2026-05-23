namespace TestClient.Source.Network.Packet;

public interface IPacket
{
	void Write(PacketBuffer stream);
	void Read(PacketBuffer stream);
}
