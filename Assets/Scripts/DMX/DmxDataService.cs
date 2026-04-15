public static class DmxDataService
{
    public static event System.Action<DmxFrame> OnDmxFrame;

    public static void PushFrame(byte[] buffer)
    {
        OnDmxFrame?.Invoke(new DmxFrame
        {
            Buffer = buffer
        });
    }
}