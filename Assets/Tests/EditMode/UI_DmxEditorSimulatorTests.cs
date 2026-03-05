using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UI_DmxEditorSimulatorTests
{
    [Test]
    public void PushFrameFromUi_WritesFirstEightChannelsToDmxBuffer()
    {
        var (simulator, receiver) = CreateSimulator();

        SetSlider(simulator, "masterDimmerSlider", 1f);
        SetSlider(simulator, "redSlider", 0.5f);
        SetSlider(simulator, "greenSlider", 0.25f);
        SetSlider(simulator, "blueSlider", 0f);
        SetSlider(simulator, "patternSlider", 0.75f);
        SetSlider(simulator, "speedSlider", 0.1f);
        SetSlider(simulator, "sizeSlider", 0.9f);
        SetSlider(simulator, "strobeSlider", 0.33f);

        simulator.PushFrameFromUi();

        Assert.That(receiver.DmxBuffer.GetChannel1Based(1), Is.EqualTo(255));
        Assert.That(receiver.DmxBuffer.GetChannel1Based(2), Is.EqualTo(128));
        Assert.That(receiver.DmxBuffer.GetChannel1Based(3), Is.EqualTo(64));
        Assert.That(receiver.DmxBuffer.GetChannel1Based(4), Is.EqualTo(0));
        Assert.That(receiver.DmxBuffer.GetChannel1Based(5), Is.EqualTo(191));
        Assert.That(receiver.DmxBuffer.GetChannel1Based(6), Is.EqualTo(26));
        Assert.That(receiver.DmxBuffer.GetChannel1Based(7), Is.EqualTo(230));
        Assert.That(receiver.DmxBuffer.GetChannel1Based(8), Is.EqualTo(84));
    }

    [Test]
    public void SetChannelValue_ClampsInputAndWritesSingleChannel()
    {
        var (simulator, receiver) = CreateSimulator();

        simulator.SetChannelValue(1, 2f);
        simulator.SetChannelValue(2, -1f);

        Assert.That(receiver.DmxBuffer.GetChannel1Based(1), Is.EqualTo(255));
        Assert.That(receiver.DmxBuffer.GetChannel1Based(2), Is.EqualTo(0));
    }

    private static (UI_DmxEditorSimulator simulator, ArtNetReceiver receiver) CreateSimulator()
    {
        var root = new GameObject("simulator-root");
        var receiver = root.AddComponent<ArtNetReceiver>();
        receiver.DmxBuffer = new DmxBuffer();

        var simulator = root.AddComponent<UI_DmxEditorSimulator>();
        typeof(UI_DmxEditorSimulator)
            .GetField("artNetReceiver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(simulator, receiver);

        return (simulator, receiver);
    }

    private static void SetSlider(UI_DmxEditorSimulator simulator, string fieldName, float normalizedValue)
    {
        var sliderGo = new GameObject(fieldName);
        sliderGo.AddComponent<RectTransform>();

        var slider = sliderGo.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.normalizedValue = normalizedValue;

        typeof(UI_DmxEditorSimulator)
            .GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(simulator, slider);
    }
}
