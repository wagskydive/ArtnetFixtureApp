using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CornerPinDmxWarpTests
{
    [Test]
    public void Update_AtZeroDmx_CollapsesCornersToCenter()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        var frame = new byte[512];
        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var warp = quad.AddComponent<CornerPinDmxWarp>();

        SetPrivateField(warp, "artNetReceiver", receiver);
        SetPrivateField(warp, "maxOffset", 0.5f);

        quad.SendMessage("Awake");
        quad.SendMessage("Update");

        var vertices = quad.GetComponent<MeshFilter>().mesh.vertices;

        Assert.That(vertices[0].x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(vertices[0].y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(vertices[1].x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(vertices[1].y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(vertices[2].x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(vertices[2].y, Is.EqualTo(0f).Within(0.001f));
        Assert.That(vertices[3].x, Is.EqualTo(0f).Within(0.001f));
        Assert.That(vertices[3].y, Is.EqualTo(0f).Within(0.001f));

        Object.DestroyImmediate(receiverGo);
        Object.DestroyImmediate(quad);
    }

    [Test]
    public void Update_AtMaxDmx_ExpandsCornersTowardScreenExtents()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        var frame = new byte[512];
        for (int channel = 8; channel <= 15; channel++)
        {
            frame[channel] = 255;
        }

        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var warp = quad.AddComponent<CornerPinDmxWarp>();

        SetPrivateField(warp, "artNetReceiver", receiver);
        SetPrivateField(warp, "maxOffset", 0.5f);

        quad.SendMessage("Awake");
        quad.SendMessage("Update");

        var vertices = quad.GetComponent<MeshFilter>().mesh.vertices;

        Assert.That(vertices[0].x, Is.EqualTo(-1f).Within(0.001f));
        Assert.That(vertices[0].y, Is.EqualTo(-1f).Within(0.001f));

        Assert.That(vertices[1].x, Is.EqualTo(1f).Within(0.001f));
        Assert.That(vertices[1].y, Is.EqualTo(-1f).Within(0.001f));

        Assert.That(vertices[2].x, Is.EqualTo(-1f).Within(0.001f));
        Assert.That(vertices[2].y, Is.EqualTo(1f).Within(0.001f));

        Assert.That(vertices[3].x, Is.EqualTo(1f).Within(0.001f));
        Assert.That(vertices[3].y, Is.EqualTo(1f).Within(0.001f));

        Object.DestroyImmediate(receiverGo);
        Object.DestroyImmediate(quad);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }
}
