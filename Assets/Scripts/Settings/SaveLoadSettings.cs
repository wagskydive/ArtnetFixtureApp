using UnityEngine;
using System;

public static class SaveLoadSettings
{
    public const string DmxChannelKey = "dmx.channel";
    public const string DmxUniverseKey = "dmx.universe";
    public const string DmxPatternKey = "dmx.pattern";
    public const string FixtureCountKey = "dmx.fixture.count";
    public const string FixtureModeKey = "dmx.fixture.mode";
    public const string PixelRowsKey = "dmx.pixel.rows";
    public const string PixelColumnsKey = "dmx.pixel.columns";
    public const string WebUiDeviceNameKey = "webui.device.name";

    public static event Action OnSettingsSaved;

    public static int LoadInt(string key, int defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public static string LoadString(string key, string defaultValue)
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public static void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public static void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public static void Save()
    {
        PlayerPrefs.Save();
        OnSettingsSaved?.Invoke();
    }
}
