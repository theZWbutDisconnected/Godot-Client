using System;
using System.Collections.Generic;
using TestClient.Source.Network.Packet.Client.Handshake;
using TestClient.Source.Network.Packet.Client.Login;
using TestClient.Source.Network.Packet.Client.Play;
using TestClient.Source.Network.Packet.Server.Login;
using TestClient.Source.Network.Packet.Server.Play;

namespace TestClient.Source.Network.Packet;

public static class PacketRegistry
{
    private static readonly Dictionary<ConnectionState, Dictionary<string, int>> _outbound = new(); // C2S
    private static readonly Dictionary<ConnectionState, Dictionary<int, Func<IPacket>>> _inbound = new(); // S2C

    static PacketRegistry()
    {
        // C2S Reg
        RegisterOutbound(ConnectionState.HandShaking, nameof(C00Handshake), 0x00);
        RegisterOutbound(ConnectionState.Login, nameof(C00LoginStart), 0x00);
        RegisterOutbound(ConnectionState.Login, nameof(C01EncryptionResponse), 0x01);
        RegisterOutbound(ConnectionState.Play, nameof(C00KeepAlive), 0x00);
        RegisterOutbound(ConnectionState.Play, nameof(C0FConfirmTransaction), 0x0F);
        RegisterOutbound(ConnectionState.Play, nameof(C17CustomPayload), 0x17);

        // S2C Reg
        RegisterInbound(ConnectionState.Login, 0x00, () => new S00Disconnect());
        RegisterInbound(ConnectionState.Login, 0x01, () => new S01EncryptionRequest());
        RegisterInbound(ConnectionState.Login, 0x02, () => new S02LoginSuccess());
        RegisterInbound(ConnectionState.Login, 0x03, () => new S03EnableCompression());
        RegisterInbound(ConnectionState.Play, 0x00, () => new S00KeepAlive());
        RegisterInbound(ConnectionState.Play, 0x01, () => new S01JoinGame());
        RegisterInbound(ConnectionState.Play, 0x32, () => new S32ConfirmTransaction());
        RegisterInbound(ConnectionState.Play, 0x40, () => new S40Disconnect());
    }

    private static void RegisterOutbound(ConnectionState state, string className, int packetId)
    {
        if (!_outbound.TryGetValue(state, out var map))
        {
            map = new Dictionary<string, int>();
            _outbound[state] = map;
        }

        if (map.TryGetValue(className, out var existingId))
            throw new InvalidOperationException(
                $"Duplicate outbound packet: [{state}] {className} already mapped to 0x{existingId:X2}");

        map[className] = packetId;
    }

    private static void RegisterInbound(ConnectionState state, int packetId, Func<IPacket> factory)
    {
        if (!_inbound.TryGetValue(state, out var map))
        {
            map = new Dictionary<int, Func<IPacket>>();
            _inbound[state] = map;
        }

        if (map.ContainsKey(packetId))
            throw new InvalidOperationException(
                $"Duplicate inbound packet: [{state}] 0x{packetId:X2}");

        map[packetId] = factory;
    }

    public static int GetOutboundPacketId(string className, ConnectionState state)
    {
        if (_outbound.TryGetValue(state, out var map) && map.TryGetValue(className, out var id))
            return id;

        throw new KeyNotFoundException(
            $"Outbound packetId not found for [{state}] {className}. Did you forget to register it in PacketRegistry?");
    }

    public static IPacket CreateInboundPacket(int packetId, ConnectionState state)
    {
        if (_inbound.TryGetValue(state, out var map) && map.TryGetValue(packetId, out var factory))
            return factory();

        return null;
    }
}