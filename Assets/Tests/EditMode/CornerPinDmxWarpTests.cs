using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CornerPinDmxWarpTests
{
    [Test]
    public void Update_WarpsQuadCorners_FromDmxChannels()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        var frame = new byte[512];
        // Start channel defaults to 9 (corner 1 X/Y)
        frame[8] = 255;  // +max x
        frame[9] = 255;  // +max y
        frame[10] = 0;   // -max x
        frame[11] = 0;   // -max y
        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var warp = quad.AddComponent<CornerPinDmxWarp>();

        SetPrivateField(warp, "artNetReceiver", receiver);
        SetPrivateField(warp, "maxOffset", 0.25f);

        quad.SendMessage("Awake");
        quad.SendMessage("Update");

        var vertices = quad.GetComponent<MeshFilter>().mesh.vertices;

        Assert.That(vertices[0].x, Is.EqualTo(-0.25f).Within(0.001f));
        Assert.That(vertices[0].y, Is.EqualTo(-0.25f).Within(0.001f));

        Assert.That(vertices[2].x, Is.EqualTo(-0.75f).Within(0.001f));
        Assert.That(vertices[2].y, Is.EqualTo(0.25f).Within(0.001f));

        Object.DestroyImmediate(receiverGo);
        Object.DestroyImmediate(quad);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }
}
