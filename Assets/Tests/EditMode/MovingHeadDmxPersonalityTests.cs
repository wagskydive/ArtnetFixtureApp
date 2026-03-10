using NUnit.Framework;
using UnityEngine;

public class MovingHeadDmxPersonalityTests
{
    [Test]
    public void Parse_MapsPanTiltCoarseFineToNormalized16BitValues()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        var frame = new byte[16];
        frame[4] = 127; // channel 5 pan coarse
        frame[5] = 255; // channel 6 pan fine
        frame[6] = 64;  // channel 7 tilt coarse
        frame[7] = 0;   // channel 8 tilt fine
        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var snapshot = MovingHeadDmxPersonality.Parse(receiver, 0f);

        Assert.That(snapshot.PanNormalized, Is.EqualTo(32767f / 65535f).Within(0.0001f));
        Assert.That(snapshot.TiltNormalized, Is.EqualTo(16384f / 65535f).Within(0.0001f));

        Object.DestroyImmediate(receiverGo);
    }

    [Test]
    public void Parse_MapsPatternSpeedParameterIrisRotateAndStrobe()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        var frame = new byte[16];
        frame[8] = 255;   // channel 9 pattern select
        frame[9] = 128;   // channel 10 speed
        frame[10] = 64;   // channel 11 parameter
        frame[11] = 255;  // channel 12 iris/scale
        frame[12] = 127;  // channel 13 rotate
        frame[13] = 255;  // channel 14 strobe
        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var snapshot = MovingHeadDmxPersonality.Parse(receiver, 0.2f);

        Assert.That(snapshot.PatternType, Is.EqualTo(19));
        Assert.That(snapshot.PatternSpeed, Is.EqualTo(Mathf.Lerp(0.1f, 8f, 128f / 255f)).Within(0.001f));
        Assert.That(snapshot.PatternSize, Is.EqualTo(Mathf.Lerp(0.5f, 8f, 64f / 255f)).Within(0.001f));
        Assert.That(snapshot.BeamSoftness, Is.EqualTo(Mathf.Lerp(0.001f, 0.5f, 64f / 255f)).Within(0.001f));
        Assert.That(snapshot.IrisScale, Is.EqualTo(1f).Within(0.001f));
        Assert.That(snapshot.RotateRadians, Is.EqualTo(Mathf.PI).Within(0.05f));
        Assert.That(snapshot.StrobeGate, Is.EqualTo(0f).Within(0.001f));

        Object.DestroyImmediate(receiverGo);
    }
}
