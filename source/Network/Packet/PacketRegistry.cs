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
		RegisterOutbound(ConnectionState.Play, nameof(ClientboundKeepAlive), 0x00);
		RegisterOutbound(ConnectionState.Play, nameof(ClientboundAnimation), 0x0A);
		RegisterOutbound(ConnectionState.Play, nameof(ClientboundPlayerStatus), 0x03);
		RegisterOutbound(ConnectionState.Play, nameof(ClientboundPlayerMove), 0x04);
		RegisterOutbound(ConnectionState.Play, nameof(ClientboundPlayerLook), 0x05);
		RegisterOutbound(ConnectionState.Play, nameof(ClientboundPlayerPosLook), 0x06);
		RegisterOutbound(ConnectionState.Play, nameof(ClientboundConfirmTransaction), 0x0F);
		RegisterOutbound(ConnectionState.Play, nameof(ClientboundAbilities), 0x13);
		RegisterOutbound(ConnectionState.Play, nameof(ClientboundSettings), 0x15);
		RegisterOutbound(ConnectionState.Play, nameof(ClientboundCustomPayload), 0x17);

		// S2C Reg
		RegisterInbound(ConnectionState.Login, 0x00, () => new S00Disconnect());
		RegisterInbound(ConnectionState.Login, 0x01, () => new S01EncryptionRequest());
		RegisterInbound(ConnectionState.Login, 0x02, () => new S02LoginSuccess());
		RegisterInbound(ConnectionState.Login, 0x03, () => new S03EnableCompression());
		RegisterInbound(ConnectionState.Play, 0x00, () => new ServerboundKeepAlive());
		RegisterInbound(ConnectionState.Play, 0x01, () => new ServerboundJoinGame());
		RegisterInbound(ConnectionState.Play, 0x0B, () => new ServerboundAnimation());
		RegisterInbound(ConnectionState.Play, 0x0C, () => new ServerboundSpawnPlayer());
		RegisterInbound(ConnectionState.Play, 0x0F, () => new ServerboundSpawnMob());
		RegisterInbound(ConnectionState.Play, 0x08, () => new ServerboundTeleport());
		RegisterInbound(ConnectionState.Play, 0x1B, () => new ServerboundEntityAttach());
		RegisterInbound(ConnectionState.Play, 0x1C, () => new ServerboundEntityMetadata());
		RegisterInbound(ConnectionState.Play, 0x12, () => new ServerboundVelocity());
		RegisterInbound(ConnectionState.Play, 0x13, () => new ServerboundDestroyEntities());
		RegisterInbound(ConnectionState.Play, 0x14, () => new ServerboundEntityStatus());
		RegisterInbound(ConnectionState.Play, 0x15, () => new ServerboundEntityStatus.ServerboundEntityMove());
		RegisterInbound(ConnectionState.Play, 0x16, () => new ServerboundEntityStatus.ServerboundEntityLook());
		RegisterInbound(ConnectionState.Play, 0x17, () => new ServerboundEntityStatus.ServerboundEntityLookMove());
		RegisterInbound(ConnectionState.Play, 0x18, () => new ServerboundEntityTeleport());
		RegisterInbound(ConnectionState.Play, 0x19, () => new ServerboundHeadLook());
		RegisterInbound(ConnectionState.Play, 0x21, () => new ServerboundChunkData());
		RegisterInbound(ConnectionState.Play, 0x22, () => new ServerboundMultiBlockChange());
		RegisterInbound(ConnectionState.Play, 0x23, () => new ServerboundBlockChange());
		RegisterInbound(ConnectionState.Play, 0x24, () => new ServerboundBlockAction());
		RegisterInbound(ConnectionState.Play, 0x26, () => new ServerboundMapChunkBulk());
		RegisterInbound(ConnectionState.Play, 0x32, () => new ServerboundConfirmTransaction());
		RegisterInbound(ConnectionState.Play, 0x39, () => new ServerboundAbilities());
		RegisterInbound(ConnectionState.Play, 0x40, () => new ServerboundDisconnect());
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
