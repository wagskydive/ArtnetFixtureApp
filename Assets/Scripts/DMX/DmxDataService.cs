using System;
using UnityEngine;
public static class DmxDataService
{
    public static event Action<DmxFrame> OnFrameReceived;

    public static DmxFrame LatestFrame { get; private set; }
    public static float LastFrameTime { get; private set; }

    public static void PushFrame(DmxFrame frame)
    {
        LatestFrame = frame;
        LastFrameTime = Time.time;

        OnFrameReceived?.Invoke(frame);
    }
}