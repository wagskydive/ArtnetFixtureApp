using NUnit.Framework;
using UnityEngine;

public class PixelMappingDmxPersonalityTests
{
    [Test]
    public void ParseMasterDimmer_ReturnsNormalizedChannelOne()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        var frame = new byte[16];
        frame[0] = 128;
        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        float dimmer = PixelMappingDmxPersonality.ParseMasterDimmer(receiver);

        Assert.That(dimmer, Is.EqualTo(128f / 255f).Within(0.001f));
        Object.DestroyImmediate(receiverGo);
    }

    [Test]
    public void ParsePixelColors_ReadsRgbTripletsStartingAtChannel11()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        var frame = new byte[32];
        frame[10] = 255; // pixel 0 R (channel 11)
        frame[11] = 128; // pixel 0 G
        frame[12] = 64;  // pixel 0 B
        frame[13] = 10;  // pixel 1 R
        frame[14] = 20;  // pixel 1 G
        frame[15] = 30;  // pixel 1 B
        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var pixels = new Color32[2];
        PixelMappingDmxPersonality.ParsePixelColors(receiver, 1, 2, pixels);

        Assert.That(pixels[0].r, Is.EqualTo(255));
        Assert.That(pixels[0].g, Is.EqualTo(128));
        Assert.That(pixels[0].b, Is.EqualTo(64));
        Assert.That(pixels[1].r, Is.EqualTo(10));
        Assert.That(pixels[1].g, Is.EqualTo(20));
        Assert.That(pixels[1].b, Is.EqualTo(30));

        Object.DestroyImmediate(receiverGo);
    }
}
