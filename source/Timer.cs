using System;
using Godot;

namespace TestClient.Source;

public class Timer
{
    private readonly float _ticksPerSecond;
    private long _counter;
    private double _lastHrTime;
    private long _lastSyncHrClock;
    private long _lastSyncSysClock;
    private double _timeSyncAdjustment = 1.0;
    public float ElapsedPartialTicks;
    public int ElapsedTicks;
    public float RenderPartialTicks;
    public float TimerSpeed = 1.0f;

    public Timer(float tps)
    {
        _ticksPerSecond = tps;
        _lastSyncSysClock = (long)Time.GetTicksMsec();
        _lastSyncHrClock = (long)(Time.GetTicksUsec() / 1000);
    }

    public void UpdateTimer()
    {
        var i = (long)Time.GetTicksMsec();
        var j = i - _lastSyncSysClock;
        var k = (long)(Time.GetTicksUsec() / 1000);
        var d0 = k / 1000.0;

        if (j <= 1000L && j >= 0L)
        {
            _counter += j;

            if (_counter > 1000L)
            {
                var l = k - _lastSyncHrClock;
                var d1 = _counter / (double)l;
                _timeSyncAdjustment += (d1 - _timeSyncAdjustment) * 0.20000000298023224;
                _lastSyncHrClock = k;
                _counter = 0L;
            }

            if (_counter < 0L) _lastSyncHrClock = k;
        }
        else
        {
            _lastHrTime = d0;
        }

        _lastSyncSysClock = i;
        var d2 = (d0 - _lastHrTime) * _timeSyncAdjustment;
        _lastHrTime = d0;
        d2 = Math.Clamp(d2, 0.0, 1.0);
        ElapsedPartialTicks = (float)(ElapsedPartialTicks + d2 * TimerSpeed * _ticksPerSecond);
        ElapsedTicks = (int)ElapsedPartialTicks;
        ElapsedPartialTicks -= ElapsedTicks;

        if (ElapsedTicks > 10) ElapsedTicks = 10;

        RenderPartialTicks = ElapsedPartialTicks;
    }
}