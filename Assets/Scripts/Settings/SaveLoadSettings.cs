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
    public const string DeviceNetworkKey = "device.network.name";
    public const string WebUiPasswordKey = "webui.password";
    public const string WebUiPasswordEnabledKey = "webui.password.enabled";
    public const string NetworkWarningEnabledKey = "network.warning.enabled";
    public const string InfoPanelEnabledKey = "info.panel.enabled";
    public const string IapEntitlementsKey = "iap.entitlements";
    public const string IapConsumablesKey = "iap.consumables";

    public static event Action OnSettingsSaved;

    public static int LoadInt(string key, int defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    public static string LoadString(string key, string defaultValue)
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    public static long LoadLong(string key, long defaultValue)
    {
        string raw = PlayerPrefs.GetString(key, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        return long.TryParse(raw, out long parsed) ? parsed : defaultValue;
    }

    public static void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public static void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    public static void SaveLong(string key, long value)
    {
        PlayerPrefs.SetString(key, value.ToString());
    }

    public static void Save()
    {
        PlayerPrefs.Save();
        OnSettingsSaved?.Invoke();
    }
}
