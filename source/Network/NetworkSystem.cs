using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Godot;
using TestClient.Source.Network.Packet;
using TestClient.Source.Network.Packet.Client;
using TestClient.Source.Network.Packet.NetHandler;
using TestClient.Source.Network.Packet.Server.Login;
using TestClient.Source.Network.Packet.Server.Play;

namespace TestClient.Source.Network;

public class NetworkSystem
{
    private readonly ClientHandler _manager = new();
    private PacketBuffer _buffer;
    private int _compressionThreshold = -1; // -1 is unenabled
    private bool _isDead;
    private bool _isReading;
    private string _username = "LocalPlayer";

    public ConnectionState State { get; private set; } = ConnectionState.HandShaking;

    public void SetUsername(string username)
    {
        _username = username;
    }

    public void SetCompressionThreshold(int threshold)
    {
        _compressionThreshold = threshold;
        GD.Print($"Compression enabled with threshold: {threshold}");
    }

    public async Task Connect(string ip, int port)
    {
        await _manager.Connect(ip, port);
        _buffer = new PacketBuffer(_manager.Stream);
        GD.Print("Connected to target: " + ip + ":" + port);
    }

    public void Disconnect()
    {
        _isDead = true;
        _manager.Disconnect();
    }

    public NetworkSystem SetState(ConnectionState state)
    {
        State = state;
        switch (state)
        {
            case ConnectionState.HandShaking:
                _manager.SetNetHandler(new NetHandlerHandshakeTcp());
                break;
            case ConnectionState.Login:
                _manager.SetNetHandler(new NetHandlerLoginClient(this, _username));
                break;
            case ConnectionState.Play:
                _manager.SetNetHandler(new NetHandlerPlayClient(this, _username));
                break;
            case ConnectionState.Status:
                break;
            default:
                throw new ArgumentOutOfRangeException("Invalid connection state.");
        }

        return this;
    }

    public void StreamProcess()
    {
        if (_isReading || _isDead || !_manager.IsConnected()) return;
        _isReading = true;

        Task.Run(() =>
        {
            try
            {
                while (_manager.IsConnected())
                {
                    var length = _buffer.ReadVarInt();
                    if (length <= 0) continue;

                    var packetData = _buffer.ReadBytes(length);
                    byte[] payload;

                    if (_compressionThreshold >= 0)
                    {
                        var tempBuf = new PacketBuffer(new MemoryStream(packetData));
                        var dataLength = tempBuf.ReadVarInt();

                        if (dataLength == 0)
                        {
                            payload = tempBuf.ReadBytes(packetData.Length - PacketBuffer.GetVarIntSize(0));
                        }
                        else
                        {
                            var headerSize = PacketBuffer.GetVarIntSize(dataLength);
                            var compressedLen = packetData.Length - headerSize;
                            var compressed = tempBuf.ReadBytes(compressedLen);

                            payload = new byte[dataLength];
                            using var deflate = new DeflateStream(
                                new MemoryStream(compressed, 2, compressed.Length - 2), // 跳过 zlib 2字节头
                                CompressionMode.Decompress);
                            deflate.ReadExactly(payload, 0, dataLength);
                        }
                    }
                    else
                    {
                        payload = packetData;
                    }

                    var reader = new PacketBuffer(new MemoryStream(payload));
                    var packetId = reader.ReadVarInt();
                    var packet = PacketRegistry.CreateInboundPacket(packetId, State);
                    if (packet != null)
                    {
                        packet.Read(reader);
                        DispatchPacket(packet);
                    }
                }
            }
            catch (Exception ex)
            {
                _isDead = true;
                _manager.PacketListener?.Disconnected(ex.Message);
            }
            finally
            {
                _isReading = false;
            }
        });
    }

    private void DispatchPacket(IPacket packet)
    {
        var handler = _manager.PacketListener;
        if (handler == null) return;

        switch (State, packet)
        {
            case (ConnectionState.Login, S00Disconnect p):
                ((INetHandlerLoginClient)handler).HandleDisconnect(p);
                break;
            case (ConnectionState.Login, S01PacketEncryptionRequest p):
                ((INetHandlerLoginClient)handler).HandleEncryptionRequest(p);
                break;
            case (ConnectionState.Login, S02PacketLoginSuccess p):
                ((INetHandlerLoginClient)handler).HandleLoginSuccess(p);
                break;
            case (ConnectionState.Login, S03PacketEnableCompression p):
                ((INetHandlerLoginClient)handler).HandleEnableCompression(p);
                break;
            case (ConnectionState.Play, S00KeepAlive p):
                ((INetHandlerPlayClient)handler).HandleKeepAlive(p);
                break;
            case (ConnectionState.Play, S01JoinGame p):
                ((INetHandlerPlayClient)handler).HandleJoinGame(p);
                break;
        }
    }

    public async Task SendPacket(IPacket packet)
    {
        if (!_manager.IsConnected())
        {
            GD.Print("Channel was closed.");
            return;
        }

        var stream = _manager.Stream;
        if (stream?.CanWrite != true) throw new IOException("Unable to write stream.");

        var packetId = PacketRegistry.GetOutboundPacketId(packet.GetType().Name, State);

        using var bodyStream = new MemoryStream();
        var bodyBuffer = new PacketBuffer(bodyStream);
        bodyBuffer.WriteVarInt(packetId);
        packet.Write(bodyBuffer);
        var bodyBytes = bodyStream.ToArray();

        byte[] frameBytes;

        if (_compressionThreshold >= 0)
        {
            if (bodyBytes.Length >= _compressionThreshold)
            {
                byte[] compressed;
                using (var compressedStream = new MemoryStream())
                {
                    compressedStream.WriteByte(0x78);
                    compressedStream.WriteByte(0x9C);
                    using (var deflate = new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
                    {
                        deflate.Write(bodyBytes, 0, bodyBytes.Length);
                    }

                    compressed = compressedStream.ToArray();
                }

                using var frameStream = new MemoryStream();
                var frameBuf = new PacketBuffer(frameStream);
                frameBuf.WriteVarInt(bodyBytes.Length);
                frameStream.Write(compressed, 0, compressed.Length);
                frameBytes = frameStream.ToArray();
            }
            else
            {
                using var frameStream = new MemoryStream();
                var frameBuf = new PacketBuffer(frameStream);
                frameBuf.WriteVarInt(0);
                frameStream.Write(bodyBytes, 0, bodyBytes.Length);
                frameBytes = frameStream.ToArray();
            }
        }
        else
        {
            frameBytes = bodyBytes;
        }

        _buffer.WriteVarInt(frameBytes.Length);
        stream.Write(frameBytes, 0, frameBytes.Length);
        await stream.FlushAsync();
    }
}