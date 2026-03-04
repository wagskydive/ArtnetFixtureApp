using System;


// Double-buffered zero-allocation DMX storage
public class DmxBuffer
{
    private readonly byte[] _frontBuffer = new byte[512];
    private readonly byte[] _backBuffer = new byte[512];

    private readonly object _lock = new object();
    private bool _hasNewFrame = false;

    public void WriteFrame(byte[] source)
    {
        lock (_lock)
        {
            Buffer.BlockCopy(source, 0, _backBuffer, 0, 512);
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

    public byte GetChannel(int index)
    {
        return _frontBuffer[index];
    }

    public byte[] GetRawBuffer()
    {
        return _frontBuffer;
    }
}

