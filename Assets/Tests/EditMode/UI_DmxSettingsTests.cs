using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class UI_DmxSettingsTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey("dmx.channel");
        PlayerPrefs.DeleteKey("dmx.universe");
        PlayerPrefs.DeleteKey("dmx.pattern");
    }

    [Test]
    public void CurrentDmxChannel_UpdatesField_WhenInRange()
    {
        var (settings, channelText, _, _) = CreateSettings();

        settings.CurrentDmxChannel = 120;

        Assert.That(settings.CurrentDmxChannel, Is.EqualTo(120));
        Assert.That(channelText.text, Is.EqualTo("120"));
    }

    [Test]
    public void CurrentDmxUniverse_UpdatesField_WhenInRange()
    {
        var (settings, _, universeText, _) = CreateSettings();

        settings.CurrentDmxUniverse = 10;

        Assert.That(settings.CurrentDmxUniverse, Is.EqualTo(10));
        Assert.That(universeText.text, Is.EqualTo("10"));
    }

    [Test]
    public void CurrentValues_IgnoreOutOfRangeInputs()
    {
        var (settings, channelText, universeText, _) = CreateSettings();

        settings.CurrentDmxChannel = 20;
        settings.CurrentDmxUniverse = 2;

        settings.CurrentDmxChannel = 0;
        settings.CurrentDmxUniverse = 17;

        Assert.That(settings.CurrentDmxChannel, Is.EqualTo(20));
        Assert.That(settings.CurrentDmxUniverse, Is.EqualTo(2));
        Assert.That(channelText.text, Is.EqualTo("20"));
        Assert.That(universeText.text, Is.EqualTo("2"));
    }

    [Test]
    public void IncreaseChannel_IncrementsValue()
    {
        var (settings, _, _, _) = CreateSettings();

        settings.CurrentDmxChannel = 255;
        settings.IncreaseChannel();

        Assert.That(settings.CurrentDmxChannel, Is.EqualTo(256));
    }

    [Test]
    public void IncreaseUniverse_IncrementsValue()
    {
        var (settings, _, _, _) = CreateSettings();

        settings.CurrentDmxUniverse = 15;
        settings.IncreaseUniverse();

        Assert.That(settings.CurrentDmxUniverse, Is.EqualTo(16));
    }

    [Test]
    public void DecreaseMethods_DoNotGoBelowMinimum()
    {
        var (settings, _, _, _) = CreateSettings();

        settings.CurrentDmxChannel = 1;
        settings.CurrentDmxUniverse = 1;
        settings.DecreaseChannel();
        settings.DecreaseUniverse();

        Assert.That(settings.CurrentDmxChannel, Is.EqualTo(1));
        Assert.That(settings.CurrentDmxUniverse, Is.EqualTo(1));
    }

    [Test]
    public void PatternSetter_UpdatesGlobalShaderValue()
    {
        var (settings, _, _, _) = CreateSettings();

        settings.CurrentPatternType = 2;

        Assert.That(Shader.GetGlobalInt("_PatternType"), Is.EqualTo(2));
    }

    [Test]
    public void PatternSetter_UsesInjectedShaderSetter()
    {
        var (settings, _, _, _) = CreateSettings();
        var fakeSetter = new FakeShaderSetter();
        settings.ShaderGlobalIntSetter = fakeSetter;

        settings.CurrentPatternType = 1;

        Assert.That(fakeSetter.LastPropertyName, Is.EqualTo("_PatternType"));
        Assert.That(fakeSetter.LastValue, Is.EqualTo(1));
    }


    [Test]
    public void ChannelAndUniverse_ApplyToReceiverSettings()
    {
        var (settings, _, _, receiver) = CreateSettings();

        settings.CurrentDmxChannel = 100;
        settings.CurrentDmxUniverse = 8;

        Assert.That(receiver.StartChannel, Is.EqualTo(100));
        Assert.That(receiver.Universe, Is.EqualTo(7));
    }


    [Test]
    public void SavePreferences_UsesReceiverAddressValuesWhenTheyWereChangedExternally()
    {
        var (settings, _, _, receiver) = CreateSettings();

        receiver.SetUniverseFromUserInput(11);
        receiver.SetStartChannelFromUserInput(222);

        settings.SavePreferences();

        Assert.That(PlayerPrefs.GetInt("dmx.universe", -1), Is.EqualTo(11));
        Assert.That(PlayerPrefs.GetInt("dmx.channel", -1), Is.EqualTo(222));
        Assert.That(settings.CurrentDmxUniverse, Is.EqualTo(11));
        Assert.That(settings.CurrentDmxChannel, Is.EqualTo(222));
    }

    [Test]
    public void SaveAndLoadPreferences_RestoresValues()
    {
        var (settings, _, _, _) = CreateSettings();
        settings.CurrentDmxChannel = 200;
        settings.CurrentDmxUniverse = 12;
        settings.CurrentPatternType = 2;
        settings.SavePreferences();

        var (reloadedSettings, _, _, _) = CreateSettings();
        reloadedSettings.LoadPreferences();

        Assert.That(reloadedSettings.CurrentDmxChannel, Is.EqualTo(200));
        Assert.That(reloadedSettings.CurrentDmxUniverse, Is.EqualTo(12));
        Assert.That(reloadedSettings.CurrentPatternType, Is.EqualTo(2));
    }

    private static (UI_DmxSettings settings, Text channelText, Text universeText, ArtNetReceiver receiver) CreateSettings()
    {
        var root = new GameObject("ui-root");
        var settings = root.AddComponent<UI_DmxSettings>();
        var receiver = root.AddComponent<ArtNetReceiver>();

        var channelGo = new GameObject("channel");
        var universeGo = new GameObject("universe");

        channelGo.AddComponent<RectTransform>();
        universeGo.AddComponent<RectTransform>();

        var channelText = channelGo.AddComponent<Text>();
        var universeText = universeGo.AddComponent<Text>();

        typeof(UI_DmxSettings)
            .GetField("channelValueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(settings, channelText);

        typeof(UI_DmxSettings)
            .GetField("universeValueText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(settings, universeText);

        typeof(UI_DmxSettings)
            .GetField("artNetReceiver", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(settings, receiver);

        return (settings, channelText, universeText, receiver);
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
