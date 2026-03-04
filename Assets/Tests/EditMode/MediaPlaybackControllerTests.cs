using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class MediaPlaybackControllerTests
{
    [Test]
    public void ResolveMediaIndex_ReturnsNegativeOne_WhenMediaListEmpty()
    {
        var go = new GameObject("media-controller");
        var controller = go.AddComponent<MediaPlaybackController>();

        int index = controller.ResolveMediaIndex(200);

        Assert.That(index, Is.EqualTo(-1));
        Object.DestroyImmediate(go);
    }

    [Test]
    public void Update_AppliesTransportAndLooping_FromDmxChannels()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        var frame = new byte[16];
        frame[8] = 0;    // channel 9: media select
        frame[9] = 200;  // channel 10: play
        frame[10] = 255; // channel 11: loop on
        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var controllerGo = new GameObject("controller");
        var controller = controllerGo.AddComponent<MediaPlaybackController>();
        var fakeBackend = new FakePlaybackBackend();

        SetPrivateField(controller, "artNetReceiver", receiver);
        SetPrivateField(controller, "mediaFiles", new List<string> { "clipA.mp4", "clipB.mp4" });
        controller.PlaybackBackend = fakeBackend;

        controllerGo.SendMessage("Update");

        Assert.That(fakeBackend.PlayCalls, Is.EqualTo(1));
        Assert.That(fakeBackend.PauseCalls, Is.EqualTo(0));
        Assert.That(fakeBackend.StopCalls, Is.EqualTo(0));
        Assert.That(fakeBackend.LastLooping, Is.True);

        Object.DestroyImmediate(controllerGo);
        Object.DestroyImmediate(receiverGo);
    }

    [Test]
    public void Update_UsesStopAndPause_TransportRanges()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        var controllerGo = new GameObject("controller");
        var controller = controllerGo.AddComponent<MediaPlaybackController>();
        var fakeBackend = new FakePlaybackBackend();

        SetPrivateField(controller, "artNetReceiver", receiver);
        SetPrivateField(controller, "mediaFiles", new List<string> { "clipA.mp4" });
        controller.PlaybackBackend = fakeBackend;

        receiver.DmxBuffer.WriteFrame(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 11); // stop
        receiver.DmxBuffer.SwapIfNewFrame();
        controllerGo.SendMessage("Update");

        receiver.DmxBuffer.WriteFrame(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 0 }, 11); // pause
        receiver.DmxBuffer.SwapIfNewFrame();
        controllerGo.SendMessage("Update");

        Assert.That(fakeBackend.StopCalls, Is.EqualTo(1));
        Assert.That(fakeBackend.PauseCalls, Is.EqualTo(1));

        Object.DestroyImmediate(controllerGo);
        Object.DestroyImmediate(receiverGo);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }

    private class FakePlaybackBackend : IVideoPlaybackBackend
    {
        public string LastUrl { get; private set; }
        public bool LastLooping { get; private set; }
        public int PlayCalls { get; private set; }
        public int PauseCalls { get; private set; }
        public int StopCalls { get; private set; }

        public void SetUrl(string url)
        {
            LastUrl = url;
        }

        public void SetLooping(bool shouldLoop)
        {
            LastLooping = shouldLoop;
        }

        public void Play()
        {
            PlayCalls++;
        }

        public void Pause()
        {
            PauseCalls++;
        }

        public void Stop()
        {
            StopCalls++;
        }
    }
}
