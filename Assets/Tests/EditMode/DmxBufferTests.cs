using NUnit.Framework;

public class DmxBufferTests
{
    [Test]
    public void WriteFrame_AndSwapIfNewFrame_CopiesExpectedBytes()
    {
        var buffer = new DmxBuffer();
        var frame = new byte[] { 10, 20, 30, 40 };

        buffer.WriteFrame(frame, frame.Length);
        buffer.SwapIfNewFrame();

        Assert.That(buffer.GetChannel1Based(1), Is.EqualTo(10));
        Assert.That(buffer.GetChannel1Based(2), Is.EqualTo(20));
        Assert.That(buffer.GetChannel1Based(3), Is.EqualTo(30));
        Assert.That(buffer.GetChannel1Based(4), Is.EqualTo(40));
    }

    [Test]
    public void WriteFrame_RespectsMaxLengthAndSourceLength()
    {
        var buffer = new DmxBuffer();
        var frame = new byte[] { 1, 2, 3 };

        buffer.WriteFrame(frame, 99);
        buffer.SwapIfNewFrame();

        Assert.That(buffer.GetChannel1Based(3), Is.EqualTo(3));
        Assert.That(buffer.GetChannel1Based(4), Is.EqualTo(0));
    }

    [Test]
    public void GetChannel1Based_ClampsOutOfRangeAccess()
    {
        var buffer = new DmxBuffer();
        var frame = new byte[512];
        frame[0] = 5;
        frame[511] = 250;

        buffer.WriteFrame(frame, frame.Length);
        buffer.SwapIfNewFrame();

        Assert.That(buffer.GetChannel1Based(0), Is.EqualTo(5));
        Assert.That(buffer.GetChannel1Based(513), Is.EqualTo(250));
    }

    [Test]
    public void GetRawBuffer_ReturnsFrontBufferReference()
    {
        var buffer = new DmxBuffer();

        var raw = buffer.GetRawBuffer();

        Assert.That(raw, Is.Not.Null);
        Assert.That(raw.Length, Is.EqualTo(512));
    }
}
