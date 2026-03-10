using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Video;

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
        frame[104] = 255; // channel 5 => highest pattern slot
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
        Assert.That(material.GetInt("_PatternType"), Is.EqualTo(19));
        Assert.That(material.GetFloat("_Size"), Is.EqualTo(8f).Within(0.001f));
        Assert.That(material.GetFloat("_StrobeGate"), Is.EqualTo(1f).Within(0.001f));

        Object.DestroyImmediate(receiverGo);
        Object.DestroyImmediate(outputGo);
    }


    [Test]
    public void PatternMediaTextureController_Update_UsesFallbackWhenVideoTextureUnavailable()
    {
        var outputGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var videoGo = new GameObject("video-player");
        var controller = outputGo.AddComponent<PatternMediaTextureController>();
        var videoPlayer = videoGo.AddComponent<VideoPlayer>();

        var fallbackTexture = new Texture2D(2, 2);
        fallbackTexture.SetPixel(0, 0, Color.magenta);
        fallbackTexture.Apply();

        SetPrivateField(controller, "outputRenderer", outputGo.GetComponent<Renderer>());
        SetPrivateField(controller, "videoPlayer", videoPlayer);
        SetPrivateField(controller, "fallbackImage", fallbackTexture);

        outputGo.SendMessage("Awake");
        outputGo.SendMessage("Update");

        var material = outputGo.GetComponent<Renderer>().material;
        Assert.That(material.GetTexture("_FallbackTex"), Is.EqualTo(fallbackTexture));
        Assert.That(material.GetFloat("_UseMediaTex"), Is.EqualTo(0f).Within(0.001f));

        Object.DestroyImmediate(fallbackTexture);
        Object.DestroyImmediate(videoGo);
        Object.DestroyImmediate(outputGo);
    }

    [Test]
    public void Update_DoesNothingWhenDependenciesAreMissing()
    {
        var go = new GameObject("missing-deps");
        var generator = go.AddComponent<PatternGenerator>();
        var output = go.AddComponent<ProjectorLightOutput>();
        var movingHead = go.AddComponent<MovingHeadBeamController>();

        Assert.DoesNotThrow(() => go.SendMessage("Update", SendMessageOptions.DontRequireReceiver));
        Assert.DoesNotThrow(() => generator.SendMessage("Update"));
        Assert.DoesNotThrow(() => output.SendMessage("Update"));
        Assert.DoesNotThrow(() => movingHead.SendMessage("Update"));

        Object.DestroyImmediate(go);
    }

    [Test]
    public void MovingHeadBeamController_Update_AppliesBeamPropertiesFromDmx()
    {
        var receiverGo = new GameObject("receiver");
        var receiver = receiverGo.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();
        receiver.StartChannel = 1;

        var frame = new byte[32];
        frame[4] = 255;   // channel 5 pan => +1 offset x
        frame[6] = 0;     // channel 7 tilt => -1 offset y
        frame[10] = 255;  // channel 11 parameter => max softness
        frame[11] = 128;  // channel 12 iris/scale => midpoint radius
        frame[12] = 127;  // channel 13 rotation => pi
        receiver.DmxBuffer.WriteFrame(frame, frame.Length);
        receiver.DmxBuffer.SwapIfNewFrame();

        var outputGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var controller = outputGo.AddComponent<MovingHeadBeamController>();

        SetPrivateField(controller, "artNetReceiver", receiver);
        SetPrivateField(controller, "outputRenderer", outputGo.GetComponent<Renderer>());

        outputGo.SendMessage("Awake");
        outputGo.SendMessage("Update");

        var material = outputGo.GetComponent<Renderer>().material;
        Assert.That(material.GetFloat("_BeamOffsetX"), Is.EqualTo(1f).Within(0.001f));
        Assert.That(material.GetFloat("_BeamOffsetY"), Is.EqualTo(-1f).Within(0.001f));
        Assert.That(material.GetFloat("_BeamSoftness"), Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(material.GetFloat("_BeamRadius"), Is.EqualTo(0.527f).Within(0.01f));
        Assert.That(material.GetFloat("_BeamRotation"), Is.EqualTo(Mathf.PI).Within(0.05f));

        Object.DestroyImmediate(receiverGo);
        Object.DestroyImmediate(outputGo);
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
