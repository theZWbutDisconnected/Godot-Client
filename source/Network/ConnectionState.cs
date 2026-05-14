using System;

namespace TestClient.Source.Network;

public enum ConnectionState
{
    HandShaking = -1,
    Play = 0,
    Status = 1,
    Login = 2
}

public static class ConnectionStateExtensions
{
    public static ConnectionState Get(int state)
    {
        return state switch
        {
            -1 => ConnectionState.HandShaking,
            0 => ConnectionState.Play,
            1 => ConnectionState.Status,
            2 => ConnectionState.Login,
            _ => throw new ArgumentOutOfRangeException(nameof(state))
        };
    }
}