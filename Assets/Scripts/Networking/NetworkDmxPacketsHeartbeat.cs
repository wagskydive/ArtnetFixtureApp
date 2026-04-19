using System;

public static class NetworkDmxPacketsHeartbeat
{
    private static long _lastPacketTicks;
    public static long LastPacketTicks => _lastPacketTicks;

    static NetworkDmxPacketsHeartbeat()
    {

        Initialize();
    }

    public static void Initialize()
    {
        _lastPacketTicks = DateTime.UtcNow.Ticks;
    }

    public static void NotifyPacketReceived()
    {
        _lastPacketTicks = DateTime.UtcNow.Ticks;
    }

}