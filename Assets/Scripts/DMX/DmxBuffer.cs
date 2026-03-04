using System;


// Double-buffered zero-allocation DMX storage
public class DmxBuffer
{
    private readonly byte[] _frontBuffer = new byte[512];
    private readonly byte[] _backBuffer = new byte[512];

    private readonly object _lock = new object();
    private bool _hasNewFrame = false;

    public void WriteFrame(byte[] source, int length)
    {
        lock (_lock)
        {
            int copyLength = Math.Min(Math.Min(length, source.Length), 512);
            Buffer.BlockCopy(source, 0, _backBuffer, 0, copyLength);
            _hasNewFrame = true;
        }
    }

    public void SwapIfNewFrame()
    {
        if (!_hasNewFrame) return;

        lock (_lock)
        {
            Buffer.BlockCopy(_backBuffer, 0, _frontBuffer, 0, 512);
            _hasNewFrame = false;
        }
    }

    public byte GetChannel1Based(int channel)
    {
        int clampedIndex = channel - 1;
        if (clampedIndex < 0) clampedIndex = 0;
        if (clampedIndex > 511) clampedIndex = 511;
        return _frontBuffer[clampedIndex];
    }

    public byte[] GetRawBuffer()
    {
        return _frontBuffer;
    }
}
