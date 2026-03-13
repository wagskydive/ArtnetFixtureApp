using UnityEngine;

[System.Serializable]
public class WebUiSettingsData
{
    public string deviceName = "ArtnetFixture";
    public string ipAddress = "0.0.0.0";
    public string fixtureMode = "surface";
    public int dmxUniverse = 1;
    public int startChannel = 1;
    public int fixtureAmount = 1;
    public int gridX = 8;
    public int gridY = 8;
    public bool passwordConfigured;
}

public static class WebUiSettingsStore
{
    public static WebUiSettingsData Load()
    {
        int fixtureMode = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.FixtureModeKey, 0), 0, 2);

        return new WebUiSettingsData
        {
            deviceName = SaveLoadSettings.LoadString(SaveLoadSettings.WebUiDeviceNameKey, "ArtnetFixture"),
            fixtureMode = ToFixtureModeValue(fixtureMode),
            dmxUniverse = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey, 1), 1, 16),
            startChannel = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.DmxChannelKey, 1), 1, 512),
            fixtureAmount = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.FixtureCountKey, 1), 1, 16),
            gridX = ClampPixelDimension(SaveLoadSettings.LoadInt(SaveLoadSettings.PixelColumnsKey, 8)),
            gridY = ClampPixelDimension(SaveLoadSettings.LoadInt(SaveLoadSettings.PixelRowsKey, 8)),
            passwordConfigured = !string.IsNullOrWhiteSpace(SaveLoadSettings.LoadString(SaveLoadSettings.WebUiPasswordKey, string.Empty))
        };
    }

    public static WebUiSettingsData Sanitize(WebUiSettingsData raw)
    {
        if (raw == null)
        {
            return Load();
        }

        return new WebUiSettingsData
        {
            deviceName = string.IsNullOrWhiteSpace(raw.deviceName) ? "ArtnetFixture" : raw.deviceName.Trim(),
            fixtureMode = NormalizeFixtureMode(raw.fixtureMode),
            dmxUniverse = Mathf.Clamp(raw.dmxUniverse, 1, 16),
            startChannel = Mathf.Clamp(raw.startChannel, 1, 512),
            fixtureAmount = Mathf.Clamp(raw.fixtureAmount, 1, 16),
            gridX = ClampPixelDimension(raw.gridX),
            gridY = ClampPixelDimension(raw.gridY),
            passwordConfigured = raw.passwordConfigured
        };
    }

    public static void Save(WebUiSettingsData raw)
    {
        WebUiSettingsData data = Sanitize(raw);

        SaveLoadSettings.SaveString(SaveLoadSettings.WebUiDeviceNameKey, data.deviceName);
        SaveLoadSettings.SaveInt(SaveLoadSettings.FixtureModeKey, ToFixtureModeIndex(data.fixtureMode));
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxUniverseKey, data.dmxUniverse);
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxChannelKey, data.startChannel);
        SaveLoadSettings.SaveInt(SaveLoadSettings.FixtureCountKey, data.fixtureAmount);
        SaveLoadSettings.SaveInt(SaveLoadSettings.PixelColumnsKey, data.gridX);
        SaveLoadSettings.SaveInt(SaveLoadSettings.PixelRowsKey, data.gridY);
        SaveLoadSettings.Save();
    }

    public static string ToJson(WebUiSettingsData data)
    {
        return JsonUtility.ToJson(Sanitize(data));
    }

    public static WebUiSettingsData FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Load();
        }

        return Sanitize(JsonUtility.FromJson<WebUiSettingsData>(json));
    }

    private static int ClampPixelDimension(int value)
    {
        int clamped = Mathf.Clamp(value, 8, 32);
        int remainder = clamped % 8;
        return remainder == 0 ? clamped : clamped - remainder;
    }

    private static string NormalizeFixtureMode(string fixtureMode)
    {
        if (fixtureMode == "moving")
        {
            return "moving";
        }

        if (fixtureMode == "pixel")
        {
            return "pixel";
        }

        return "surface";
    }

    private static int ToFixtureModeIndex(string fixtureMode)
    {
        if (fixtureMode == "moving")
        {
            return 1;
        }

        if (fixtureMode == "pixel")
        {
            return 2;
        }

        return 0;
    }

    private static string ToFixtureModeValue(int fixtureMode)
    {
        if (fixtureMode == 1)
        {
            return "moving";
        }

        if (fixtureMode == 2)
        {
            return "pixel";
        }

        return "surface";
    }
}
