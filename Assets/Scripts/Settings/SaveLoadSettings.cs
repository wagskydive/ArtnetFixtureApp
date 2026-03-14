using UnityEngine;
using System;
using System.Text;

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
    public const string WebUiPasswordKey = "webui.password";
    public const string WebUiPasswordEnabledKey = "webui.password.enabled";
    public const string NetworkWarningEnabledKey = "network.warning.enabled";
    public const string InfoPanelEnabledKey = "info.panel.enabled";

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



public static class WebUiPasswordProtection
{
    public static bool IsEnabled()
    {
        return SaveLoadSettings.LoadInt(SaveLoadSettings.WebUiPasswordEnabledKey, 0) == 1;
    }

    public static string GetStoredPassword()
    {
        return SaveLoadSettings.LoadString(SaveLoadSettings.WebUiPasswordKey, string.Empty);
    }

    public static void SetEnabled(bool enabled)
    {
        SaveLoadSettings.SaveInt(SaveLoadSettings.WebUiPasswordEnabledKey, enabled ? 1 : 0);
        SaveLoadSettings.Save();
    }

    public static bool HasConfiguredPassword()
    {
        return !string.IsNullOrWhiteSpace(SaveLoadSettings.LoadString(SaveLoadSettings.WebUiPasswordKey, string.Empty));
    }

    public static bool SetPassword(string rawPassword)
    {
        string trimmed = string.IsNullOrWhiteSpace(rawPassword) ? string.Empty : rawPassword.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            SaveLoadSettings.SaveString(SaveLoadSettings.WebUiPasswordKey, rawPassword);
            SaveLoadSettings.Save();
            return false;
        }
        SaveLoadSettings.SaveString(SaveLoadSettings.WebUiPasswordKey, rawPassword);
        SaveLoadSettings.Save();
        return true;
    }

    public static bool VerifyPassword(string providedPassword)
    {
        string storedPassword = SaveLoadSettings.LoadString(SaveLoadSettings.WebUiPasswordKey, string.Empty);
        if (string.IsNullOrWhiteSpace(storedPassword))
        {
            return false;
        }


        return string.Equals(storedPassword, providedPassword);
    }

    public static void MigrateLegacyPasswordIfNeeded()
    {
        if (HasConfiguredPassword())
        {
            return;
        }

        string legacy = SaveLoadSettings.LoadString(SaveLoadSettings.WebUiPasswordKey, string.Empty);
        if (string.IsNullOrWhiteSpace(legacy))
        {
            return;
        }

        SetPassword(legacy);
    }


}
