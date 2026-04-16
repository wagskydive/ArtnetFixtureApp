using System;

public static class NetworkDataEvents
{
    public static event Action NoDataReceivedRecently;

    public static event Action DataReceivedAgain;

    public static void RaiseNoDataEvent()
    {
        NoDataReceivedRecently?.Invoke();
    }

    public static void RaiseDataBackEvent()
    {
        DataReceivedAgain?.Invoke();
    }
}