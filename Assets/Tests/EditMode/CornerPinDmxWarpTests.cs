using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CornerPinDmxWarpTests
{
    [Test]
    public void Update_AtZeroDmx_CollapsesEntireMeshToLowerLeftCorner()
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
        SetPrivateField(warp, "subdivisionAmount", 4);

        quad.SendMessage("Awake");
        quad.SendMessage("Update");

        var vertices = quad.GetComponent<MeshFilter>().mesh.vertices;

        foreach (var vertex in vertices)
        {
            Assert.That(vertex.x, Is.EqualTo(-1f).Within(0.001f));
            Assert.That(vertex.y, Is.EqualTo(-1f).Within(0.001f));
        }

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
        SetPrivateField(warp, "subdivisionAmount", 4);

        quad.SendMessage("Awake");
        quad.SendMessage("Update");

        var vertices = quad.GetComponent<MeshFilter>().mesh.vertices;
        int rowLength = 5;

        AssertCorner(vertices[0], -1f, -1f);
        AssertCorner(vertices[rowLength * 4], -1f, 1f);
        AssertCorner(vertices[(rowLength * 5) - 1], 1f, 1f);
        AssertCorner(vertices[rowLength - 1], 1f, -1f);

        Object.DestroyImmediate(receiverGo);
        Object.DestroyImmediate(quad);
    }

    [Test]
    public void Awake_UsesSubdivisionAmountToBuildGridMesh()
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var warp = quad.AddComponent<CornerPinDmxWarp>();

        SetPrivateField(warp, "subdivisionAmount", 6);

        quad.SendMessage("Awake");

        Mesh mesh = quad.GetComponent<MeshFilter>().mesh;
        Assert.That(mesh.vertexCount, Is.EqualTo(49));
        Assert.That(mesh.triangles.Length, Is.EqualTo(216));

        Object.DestroyImmediate(quad);
    }

    [Test]
    public void Update_AllowsAnyCornerToCrossScreenMidpoint()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        var frame = new byte[512];
        frame[12] = 64;  // top-right x near left side (channel 13)
        frame[13] = 255; // top-right y at top edge (channel 14)

        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var warp = quad.AddComponent<CornerPinDmxWarp>();

        SetPrivateField(warp, "artNetReceiver", receiver);
        SetPrivateField(warp, "maxOffset", 0.5f);
        SetPrivateField(warp, "subdivisionAmount", 4);

        quad.SendMessage("Awake");
        quad.SendMessage("Update");

        var vertices = quad.GetComponent<MeshFilter>().mesh.vertices;
        int rowLength = 5;
        Vector3 topRightVertex = vertices[(rowLength * 5) - 1];

        Assert.That(topRightVertex.x, Is.LessThan(0f));
        Assert.That(topRightVertex.y, Is.EqualTo(1f).Within(0.001f));

        Object.DestroyImmediate(receiverGo);
        Object.DestroyImmediate(quad);
    }




    private static void AssertCorner(Vector3 vertex, float expectedX, float expectedY)
    {
        Assert.That(vertex.x, Is.EqualTo(expectedX).Within(0.001f));
        Assert.That(vertex.y, Is.EqualTo(expectedY).Within(0.001f));
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }
}
