using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using TestClient.Source.Network.NetHandler;

namespace TestClient.Source.Network;

public class ClientHandler : IDisposable
{
	private TcpClient _client;

	public NetworkStream Stream { get; private set; }

	public INetHandler PacketListener { get; private set; }

	public async Task Connect(string host, int port)
	{
		_client = new TcpClient();
		await _client.ConnectAsync(host, port);
		Stream = _client.GetStream();
	}

	public void SetNetHandler(INetHandler handler)
	{
		PacketListener = handler;
	}

	public void Disconnect()
	{
		Stream?.Dispose();
		_client?.Dispose();
		Stream = null;
		_client = null;
	}

	public bool IsConnected()
	{
		return _client?.Connected == true && Stream != null;
	}

	public void Dispose()
	{
		Disconnect();
	}
}
