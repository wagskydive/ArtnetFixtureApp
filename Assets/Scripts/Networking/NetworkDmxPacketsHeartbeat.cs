using System;

public static class NetworkDmxPacketsHeartbeat
{
    private static long _lastPacketTicks;
    public static long LastPacketTicks => _lastPacketTicks;

    public static void NotifyPacketReceived()
    {
        _lastPacketTicks = DateTime.UtcNow.Ticks;
    }

}