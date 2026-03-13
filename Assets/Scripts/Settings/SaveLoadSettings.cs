using UnityEngine;
using System;
using System.Security.Cryptography;
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
    public const string WebUiPasswordLegacyKey = "webui.password";
    public const string WebUiPasswordHashKey = "webui.password.hash";
    public const string WebUiPasswordEnabledKey = "webui.password.enabled";

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

    public static void SetEnabled(bool enabled)
    {
        SaveLoadSettings.SaveInt(SaveLoadSettings.WebUiPasswordEnabledKey, enabled ? 1 : 0);
        SaveLoadSettings.Save();
    }

    public static bool HasConfiguredPassword()
    {
        return !string.IsNullOrWhiteSpace(SaveLoadSettings.LoadString(SaveLoadSettings.WebUiPasswordHashKey, string.Empty));
    }

    public static bool SetPassword(string rawPassword)
    {
        string trimmed = string.IsNullOrWhiteSpace(rawPassword) ? string.Empty : rawPassword.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        string hash = ComputeSha256Hex(trimmed);
        SaveLoadSettings.SaveString(SaveLoadSettings.WebUiPasswordHashKey, hash);
        SaveLoadSettings.SaveString(SaveLoadSettings.WebUiPasswordLegacyKey, string.Empty);
        SaveLoadSettings.Save();
        return true;
    }

    public static bool VerifyPassword(string rawPassword)
    {
        string storedHash = SaveLoadSettings.LoadString(SaveLoadSettings.WebUiPasswordHashKey, string.Empty);
        if (string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        string provided = string.IsNullOrWhiteSpace(rawPassword) ? string.Empty : rawPassword.Trim();
        string providedHash = ComputeSha256Hex(provided);
        return string.Equals(storedHash, providedHash, StringComparison.OrdinalIgnoreCase);
    }

    public static void MigrateLegacyPasswordIfNeeded()
    {
        if (HasConfiguredPassword())
        {
            return;
        }

        string legacy = SaveLoadSettings.LoadString(SaveLoadSettings.WebUiPasswordLegacyKey, string.Empty);
        if (string.IsNullOrWhiteSpace(legacy))
        {
            return;
        }

        SetPassword(legacy);
    }

    private static string ComputeSha256Hex(string value)
    {
        byte[] source = Encoding.UTF8.GetBytes(value ?? string.Empty);
        using (SHA256 sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(source);
            var builder = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
