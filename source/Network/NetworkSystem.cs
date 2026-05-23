using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Godot;
using TestClient.Source.Network.NetHandler;
using TestClient.Source.Network.Packet;
using TestClient.Source.Network.Packet.Server.Login;
using TestClient.Source.Network.Packet.Server.Play;

namespace TestClient.Source.Network;

public class NetworkSystem
{
    private readonly ClientHandler _manager = new();
    private PacketBuffer _buffer;
    private int _compressionThreshold = -1; // -1 is unenabled

    private Dictionary<Type, Action<IPacket>> _handlers = new();
    private bool _isDead;
    public string Username { get; private set; }

    public ConnectionState State { get; private set; } = ConnectionState.HandShaking;

    public bool IsConnected()
    {
        return _manager.IsConnected();
    }

    public void SetUsername(string username)
    {
        Username = username;
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
        return this;
    }

    public void SetHandler(INetHandler handler)
    {
        _manager.SetNetHandler(handler);
        _handlers = handler switch
        {
            INetHandlerLoginClient h => new Dictionary<Type, Action<IPacket>>
            {
                { typeof(S00Disconnect), p => h.HandleDisconnect((S00Disconnect)p) },
                { typeof(S01EncryptionRequest), p => h.HandleEncryptionRequest((S01EncryptionRequest)p) },
                { typeof(S02LoginSuccess), p => h.HandleLoginSuccess((S02LoginSuccess)p) },
                { typeof(S03EnableCompression), p => h.HandleEnableCompression((S03EnableCompression)p) }
            },
            INetHandlerPlayClient h => new Dictionary<Type, Action<IPacket>>
            {
                { typeof(S00KeepAlive), p => h.HandleKeepAlive((S00KeepAlive)p) },
                { typeof(S01JoinGame), p => h.HandleJoinGame((S01JoinGame)p) },
                { typeof(S08PlayerPosLook), p => h.HandlePlayerPosLook((S08PlayerPosLook)p) },
                { typeof(S12EntityVelocity), p => h.HandleEntityVelocity((S12EntityVelocity)p) },
                { typeof(S18EntityTeleport), p => h.HandleEntityTeleport((S18EntityTeleport)p) },
                { typeof(S19EntityHeadLook), p => h.HandleEntityHeadLook((S19EntityHeadLook)p) },
                { typeof(S21ChunkData), p => h.HandleChunkData((S21ChunkData)p) },
                { typeof(S22MultiBlockChange), p => h.HandleMultiBlockChange((S22MultiBlockChange)p) },
                { typeof(S23BlockChange), p => h.HandleBlockChange((S23BlockChange)p) },
                { typeof(S26MapChunkBulk), p => h.HandleMapChunkBulk((S26MapChunkBulk)p) },
                { typeof(S32ConfirmTransaction), p => h.HandleConfirmTransaction((S32ConfirmTransaction)p) },
                { typeof(S40Disconnect), p => h.HandleDisconnect((S40Disconnect)p) }
            },
            _ => _handlers
        };
    }

    public void StreamProcess()
    {
        if (_isDead || !_manager.IsConnected()) return;

        try
        {
            var length = _buffer.ReadVarInt();
            if (length <= 0) return;

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
                        new MemoryStream(compressed, 2, compressed.Length - 2),
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
        catch (Exception ex)
        {
            _isDead = true;
            _manager.PacketListener?.Disconnected(ex.Message);
        }
    }

    private void DispatchPacket(IPacket packet)
    {
        if (_handlers.TryGetValue(packet.GetType(), out var action))
            action(packet);
        else
            GD.Print($"Unhandled: {packet.GetType().Name}");
    }

    public async Task SendPacket(IPacket packet)
    {
        if (!_manager.IsConnected())
        {
            GD.PushError("Channel was closed.");
            return;
        }

        var stream = _manager.Stream;
        if (stream?.CanWrite != true)
        {
            GD.PushError("Unable to write stream.");
            return;
        }

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