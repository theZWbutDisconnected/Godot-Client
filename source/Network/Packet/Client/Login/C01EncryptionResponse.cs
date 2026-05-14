namespace TestClient.Source.Network.Packet.Client.Login;

public class C01EncryptionResponse(byte[] sharedSecretIn, byte[] verifyTokenIn) : IPacket
{
    public byte[] SharedSecret { get; } = sharedSecretIn;
    public byte[] VerifyToken { get; } = verifyTokenIn;

    public void Read(PacketBuffer buf)
    {
        /* C2S only, no need to read */
    }

    public void Write(PacketBuffer buf)
    {
        buf.WriteBytes(SharedSecret);
        buf.WriteBytes(VerifyToken);
    }
}