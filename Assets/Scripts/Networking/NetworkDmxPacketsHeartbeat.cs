using System;

public static class NetworkDmxPacketsHeartbeat
{
    private static long _lastPacketTicks;
    private static long _lastResetTicks;
    public static long LastPacketTicks => _lastPacketTicks;
    public static long LastResetTicks => _lastResetTicks;

    static NetworkDmxPacketsHeartbeat()
    {

        Initialize();
    }

    public static void Initialize()
    {
        long nowTicks = DateTime.UtcNow.Ticks;
        _lastPacketTicks = nowTicks;
        _lastResetTicks = nowTicks;
    }

    public static void NotifyPacketReceived()
    {
        _lastPacketTicks = DateTime.UtcNow.Ticks;
    }

}
