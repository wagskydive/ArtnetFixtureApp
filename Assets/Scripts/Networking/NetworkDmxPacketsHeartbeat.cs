using System;
using System.Threading;

public static class NetworkDmxPacketsHeartbeat
{
    private static long _lastPacketTicks;
    private static long _lastResetTicks;
    public static long LastPacketTicks => Interlocked.Read(ref _lastPacketTicks);
    public static long LastResetTicks => Interlocked.Read(ref _lastResetTicks);

    static NetworkDmxPacketsHeartbeat()
    {

        Initialize();
    }

    public static void Initialize()
    {
        long nowTicks = DateTime.UtcNow.Ticks;
        Interlocked.Exchange(ref _lastPacketTicks, nowTicks);
        Interlocked.Exchange(ref _lastResetTicks, nowTicks);
    }

    public static void NotifyPacketReceived()
    {
        Interlocked.Exchange(ref _lastPacketTicks, DateTime.UtcNow.Ticks);
    }

}
