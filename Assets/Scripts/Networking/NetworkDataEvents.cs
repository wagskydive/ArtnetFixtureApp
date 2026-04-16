using System;

public static class NetworkDataEvents
{
    public static event Action OnNetworkLost;

    public static event Action OnNetworkRestored;

    public static void RaiseNetworkLostEvent()
    {
        OnNetworkLost?.Invoke();
    }

    public static void RaiseNetworkRestoredEvent()
    {
        OnNetworkRestored?.Invoke();
    }
}