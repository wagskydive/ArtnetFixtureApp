using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UI_DmxSettingsTests
{
    [Test]
    public void CurrentDmxChannel_UpdatesField_WhenInRange()
    {
        var (settings, channelField, _) = CreateSettings();

        settings.CurrentDmxChannel = 120;

        Assert.That(settings.CurrentDmxChannel, Is.EqualTo(120));
        Assert.That(channelField.text, Is.EqualTo("120"));
    }

    [Test]
    public void CurrentDmxUniverse_UpdatesField_WhenInRange()
    {
        var (settings, _, universeField) = CreateSettings();

        settings.CurrentDmxUniverse = 10;

        Assert.That(settings.CurrentDmxUniverse, Is.EqualTo(10));
        Assert.That(universeField.text, Is.EqualTo("10"));
    }

    [Test]
    public void CurrentValues_IgnoreOutOfRangeInputs()
    {
        var (settings, channelField, universeField) = CreateSettings();

        settings.CurrentDmxChannel = 20;
        settings.CurrentDmxUniverse = 2;

        settings.CurrentDmxChannel = 0;
        settings.CurrentDmxUniverse = 17;

        Assert.That(settings.CurrentDmxChannel, Is.EqualTo(20));
        Assert.That(settings.CurrentDmxUniverse, Is.EqualTo(2));
        Assert.That(channelField.text, Is.EqualTo("20"));
        Assert.That(universeField.text, Is.EqualTo("2"));
    }

    [Test]
    public void OnChannelValueChanged_ParsesValidInput()
    {
        var (settings, _, _) = CreateSettings();

        settings.OnChannelValueChanged("256");

        Assert.That(settings.CurrentDmxChannel, Is.EqualTo(256));
    }

    [Test]
    public void OnUniverseValueChanged_ParsesValidInput()
    {
        var (settings, _, _) = CreateSettings();

        settings.OnUniverseValueChanged("16");

        Assert.That(settings.CurrentDmxUniverse, Is.EqualTo(16));
    }

    [Test]
    public void PatternSetter_UpdatesGlobalShaderValue()
    {
        var (settings, _, _) = CreateSettings();

        settings.CurrentPatternType = 2;

        Assert.That(Shader.GetGlobalInt("_PatternType"), Is.EqualTo(2));
    }

    [Test]
    public void PatternSetter_UsesInjectedShaderSetter()
    {
        var (settings, _, _) = CreateSettings();
        var fakeSetter = new FakeShaderSetter();
        settings.ShaderGlobalIntSetter = fakeSetter;

        settings.CurrentPatternType = 1;

        Assert.That(fakeSetter.LastPropertyName, Is.EqualTo("_PatternType"));
        Assert.That(fakeSetter.LastValue, Is.EqualTo(1));
    }

    private static (UI_DmxSettings settings, InputField channelField, InputField universeField) CreateSettings()
    {
        var root = new GameObject("ui-root");
        var settings = root.AddComponent<UI_DmxSettings>();

        var channelGo = new GameObject("channel");
        var universeGo = new GameObject("universe");
        var textGoA = new GameObject("textA");
        var textGoB = new GameObject("textB");

        channelGo.AddComponent<RectTransform>();
        universeGo.AddComponent<RectTransform>();
        textGoA.AddComponent<RectTransform>();
        textGoB.AddComponent<RectTransform>();

        var channelText = textGoA.AddComponent<Text>();
        var universeText = textGoB.AddComponent<Text>();

        var channelField = channelGo.AddComponent<InputField>();
        var universeField = universeGo.AddComponent<InputField>();
        channelField.textComponent = channelText;
        universeField.textComponent = universeText;

        typeof(UI_DmxSettings)
            .GetField("channelInputField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(settings, channelField);

        typeof(UI_DmxSettings)
            .GetField("universeInputField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(settings, universeField);

        return (settings, channelField, universeField);
    }

    private class FakeShaderSetter : IShaderGlobalIntSetter
    {
        public string LastPropertyName { get; private set; }
        public int LastValue { get; private set; }

        public void SetGlobalInt(string propertyName, int value)
        {
            LastPropertyName = propertyName;
            LastValue = value;
        }
    }
}
