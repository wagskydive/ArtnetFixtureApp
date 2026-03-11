using UnityEngine;

[System.Serializable]
public class WebUiSettingsData
{
    public string deviceName = "ArtnetFixture";
    public string fixtureMode = "surface";
    public int dmxUniverse = 1;
    public int startChannel = 1;
    public int fixtureAmount = 1;
    public int gridX = 8;
    public int gridY = 8;
}

public static class WebUiSettingsStore
{
    private const string DeviceNamePrefKey = "webui.device.name";
    private const string FixtureModePrefKey = "dmx.fixture.mode";
    private const string DmxUniversePrefKey = "dmx.universe";
    private const string StartChannelPrefKey = "dmx.channel";
    private const string FixtureCountPrefKey = "dmx.fixture.count";
    private const string PixelRowsPrefKey = "dmx.pixel.rows";
    private const string PixelColumnsPrefKey = "dmx.pixel.columns";

    public static WebUiSettingsData Load()
    {
        int fixtureMode = Mathf.Clamp(PlayerPrefs.GetInt(FixtureModePrefKey, 0), 0, 2);

        return new WebUiSettingsData
        {
            deviceName = PlayerPrefs.GetString(DeviceNamePrefKey, "ArtnetFixture"),
            fixtureMode = ToFixtureModeValue(fixtureMode),
            dmxUniverse = Mathf.Clamp(PlayerPrefs.GetInt(DmxUniversePrefKey, 1), 1, 16),
            startChannel = Mathf.Clamp(PlayerPrefs.GetInt(StartChannelPrefKey, 1), 1, 512),
            fixtureAmount = Mathf.Clamp(PlayerPrefs.GetInt(FixtureCountPrefKey, 1), 1, 16),
            gridX = ClampPixelDimension(PlayerPrefs.GetInt(PixelColumnsPrefKey, 8)),
            gridY = ClampPixelDimension(PlayerPrefs.GetInt(PixelRowsPrefKey, 8))
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
            gridY = ClampPixelDimension(raw.gridY)
        };
    }

    public static void Save(WebUiSettingsData raw)
    {
        WebUiSettingsData data = Sanitize(raw);

        PlayerPrefs.SetString(DeviceNamePrefKey, data.deviceName);
        PlayerPrefs.SetInt(FixtureModePrefKey, ToFixtureModeIndex(data.fixtureMode));
        PlayerPrefs.SetInt(DmxUniversePrefKey, data.dmxUniverse);
        PlayerPrefs.SetInt(StartChannelPrefKey, data.startChannel);
        PlayerPrefs.SetInt(FixtureCountPrefKey, data.fixtureAmount);
        PlayerPrefs.SetInt(PixelColumnsPrefKey, data.gridX);
        PlayerPrefs.SetInt(PixelRowsPrefKey, data.gridY);
        PlayerPrefs.Save();
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
