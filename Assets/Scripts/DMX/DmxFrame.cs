public struct DmxFrame
{
    public byte[] Buffer;

    public byte GetChannel(int index1Based)
    {
        int i = index1Based - 1;
        if (Buffer == null || i < 0 || i >= Buffer.Length)
            return 0;

        return Buffer[i];
    }
}