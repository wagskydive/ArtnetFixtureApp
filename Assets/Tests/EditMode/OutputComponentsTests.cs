using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class OutputComponentsTests
{
    [Test]
    public void ProjectorLightOutput_Update_AppliesColorAndIntensityFromDmx()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();
        receiver.StartChannel = 10;
        var frame = new byte[16];
        frame[9] = 128;
        frame[10] = 255;
        frame[11] = 64;
        frame[12] = 32;
        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var outputGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var output = outputGo.AddComponent<ProjectorLightOutput>();

        SetPrivateField(output, "artNetReceiver", receiver);
        SetPrivateField(output, "outputRenderer", outputGo.GetComponent<Renderer>());

        outputGo.SendMessage("Awake");
        outputGo.SendMessage("Update");

        var material = outputGo.GetComponent<Renderer>().material;
        Assert.That(material.GetFloat("_Intensity"), Is.EqualTo(128f / 255f).Within(0.001f));

        var color = material.GetColor("_Color");
        Assert.That(color.r, Is.EqualTo(1f).Within(0.001f));
        Assert.That(color.g, Is.EqualTo(64f / 255f).Within(0.001f));
        Assert.That(color.b, Is.EqualTo(32f / 255f).Within(0.001f));

        Object.DestroyImmediate(receiverGo);
        Object.DestroyImmediate(outputGo);
    }


    [Test]
    public void RgbDmxController_Update_AppliesColorAndIntensityFromDmx()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();
        receiver.StartChannel = 20;
        var frame = new byte[32];
        frame[19] = 200;
        frame[20] = 128;
        frame[21] = 64;
        frame[22] = 32;
        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var outputGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var controller = outputGo.AddComponent<RgbDmxController>();

        SetPrivateField(controller, "artNetReceiver", receiver);
        SetPrivateField(controller, "outputRenderer", outputGo.GetComponent<Renderer>());

        outputGo.SendMessage("Awake");
        outputGo.SendMessage("Update");

        var material = outputGo.GetComponent<Renderer>().material;
        Assert.That(material.GetFloat("_Intensity"), Is.EqualTo(200f / 255f).Within(0.001f));

        var color = material.GetColor("_Color");
        Assert.That(color.r, Is.EqualTo(128f / 255f).Within(0.001f));
        Assert.That(color.g, Is.EqualTo(64f / 255f).Within(0.001f));
        Assert.That(color.b, Is.EqualTo(32f / 255f).Within(0.001f));

        Object.DestroyImmediate(receiverGo);
        Object.DestroyImmediate(outputGo);
    }

    [Test]
    public void PatternGenerator_Update_AppliesPatternControlsFromDmx()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        receiver.StartChannel = 100;
        var frame = new byte[128];
        frame[104] = 215; // channel 5 => pattern 5
        frame[105] = 127; // channel 6 => speed midpoint
        frame[106] = 255; // channel 7 => max size
        frame[107] = 0;   // channel 8 => strobe off, gate 1
        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var outputGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var generator = outputGo.AddComponent<PatternGenerator>();
        SetPrivateField(generator, "artNetReceiver", receiver);
        SetPrivateField(generator, "outputRenderer", outputGo.GetComponent<Renderer>());

        outputGo.SendMessage("Awake");
        outputGo.SendMessage("Update");

        var material = outputGo.GetComponent<Renderer>().material;
        Assert.That(material.GetInt("_PatternType"), Is.EqualTo(5));
        Assert.That(material.GetFloat("_Size"), Is.EqualTo(8f).Within(0.001f));
        Assert.That(material.GetFloat("_StrobeGate"), Is.EqualTo(1f).Within(0.001f));

        Object.DestroyImmediate(receiverGo);
        Object.DestroyImmediate(outputGo);
    }

    [Test]
    public void Update_DoesNothingWhenDependenciesAreMissing()
    {
        var go = new GameObject("missing-deps");
        var generator = go.AddComponent<PatternGenerator>();
        var output = go.AddComponent<ProjectorLightOutput>();

        Assert.DoesNotThrow(() => go.SendMessage("Update", SendMessageOptions.DontRequireReceiver));
        Assert.DoesNotThrow(() => generator.SendMessage("Update"));
        Assert.DoesNotThrow(() => output.SendMessage("Update"));

        Object.DestroyImmediate(go);
    }

    [Test]
    public void ProjectorLightOutput_Update_ReducesIntensityAtSustainedHighLoad()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();
        receiver.StartChannel = 3;
        var frame = new byte[16];
        frame[2] = 255;
        frame[3] = 255;
        frame[4] = 255;
        frame[5] = 255;
        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var outputGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var output = outputGo.AddComponent<ProjectorLightOutput>();

        SetPrivateField(output, "artNetReceiver", receiver);
        SetPrivateField(output, "outputRenderer", outputGo.GetComponent<Renderer>());
        SetPrivateField(output, "thermalRampPerSecond", 30f);
        SetPrivateField(output, "thermalMinimumScale", 0.8f);

        outputGo.SendMessage("Awake");
        outputGo.SendMessage("Update");

        var material = outputGo.GetComponent<Renderer>().material;
        Assert.That(material.GetFloat("_Intensity"), Is.EqualTo(0.8f).Within(0.001f));

        Object.DestroyImmediate(receiverGo);
        Object.DestroyImmediate(outputGo);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }
}
