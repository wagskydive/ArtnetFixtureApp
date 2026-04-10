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
    public bool advancedNetworkingUnlocked;
    public int networkMode = NetworkingModeManager.ArtNetModeIndex;
    public bool useMulticast = true;
    public string multicastAddress = "239.255.0.1";
    public string unicastBindAddress = "0.0.0.0";
    public int listenPort = 5568;
    public float timeoutSeconds = 2f;
    public bool useLtpMerge;
    public string additionalUniverses = string.Empty;
    public bool showNetworkDebug;
    public bool passwordConfigured;
    public bool passwordEnabled;
}

public static class WebUiSettingsStore
{
    public static WebUiSettingsData Load()
    {
        int fixtureMode = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.FixtureModeKey, 0), 0, 2);

        return new WebUiSettingsData
        {
            deviceName = SaveLoadSettings.LoadString(SaveLoadSettings.DeviceNetworkKey, "DMX Projector"),
            fixtureMode = ToFixtureModeValue(fixtureMode),
            dmxUniverse = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey, 1), 1, 63999),
            startChannel = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.DmxChannelKey, 1), 1, 512),
            fixtureAmount = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.FixtureCountKey, 1), 1, 16),
            gridX = ClampPixelDimension(SaveLoadSettings.LoadInt(SaveLoadSettings.PixelColumnsKey, 8)),
            gridY = ClampPixelDimension(SaveLoadSettings.LoadInt(SaveLoadSettings.PixelRowsKey, 8)),
            networkMode = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.NetworkModeKey, NetworkingModeManager.ArtNetModeIndex), NetworkingModeManager.ArtNetModeIndex, NetworkingModeManager.SAcnModeIndex),
            useMulticast = SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnUseMulticastKey, 1) == 1,
            multicastAddress = SaveLoadSettings.LoadString(SaveLoadSettings.SAcnMulticastAddressKey, "239.255.0.1"),
            unicastBindAddress = SaveLoadSettings.LoadString(SaveLoadSettings.SAcnUnicastBindAddressKey, "0.0.0.0"),
            listenPort = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnListenPortKey, 5568), 1, 65535),
            timeoutSeconds = Mathf.Max(0.1f, PlayerPrefs.GetFloat(SaveLoadSettings.SAcnTimeoutSecondsKey, 2f)),
            useLtpMerge = SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnUseLtpMergeKey, 0) == 1,
            additionalUniverses = SaveLoadSettings.LoadString(SaveLoadSettings.SAcnMulticastUniversesKey, string.Empty),
            showNetworkDebug = SaveLoadSettings.LoadInt(SaveLoadSettings.SAcnDebugVisibleKey, 0) == 1,
            passwordConfigured = WebUiPasswordProtection.HasConfiguredPassword(),
            passwordEnabled = WebUiPasswordProtection.IsEnabled()
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
            ipAddress = string.IsNullOrWhiteSpace(raw.ipAddress) ? "127.0.0.1" : raw.ipAddress.Trim(),
            fixtureMode = NormalizeFixtureMode(raw.fixtureMode),
            dmxUniverse = Mathf.Clamp(raw.dmxUniverse, 1, 63999),
            startChannel = Mathf.Clamp(raw.startChannel, 1, 512),
            fixtureAmount = Mathf.Clamp(raw.fixtureAmount, 1, 16),
            gridX = ClampPixelDimension(raw.gridX),
            gridY = ClampPixelDimension(raw.gridY),
            advancedNetworkingUnlocked = raw.advancedNetworkingUnlocked,
            networkMode = Mathf.Clamp(raw.networkMode, NetworkingModeManager.ArtNetModeIndex, NetworkingModeManager.SAcnModeIndex),
            useMulticast = raw.useMulticast,
            multicastAddress = string.IsNullOrWhiteSpace(raw.multicastAddress) ? "239.255.0.1" : raw.multicastAddress.Trim(),
            unicastBindAddress = string.IsNullOrWhiteSpace(raw.unicastBindAddress) ? "0.0.0.0" : raw.unicastBindAddress.Trim(),
            listenPort = Mathf.Clamp(raw.listenPort, 1, 65535),
            timeoutSeconds = Mathf.Max(0.1f, raw.timeoutSeconds),
            useLtpMerge = raw.useLtpMerge,
            additionalUniverses = string.IsNullOrWhiteSpace(raw.additionalUniverses) ? string.Empty : raw.additionalUniverses.Trim(),
            showNetworkDebug = raw.showNetworkDebug,
            passwordConfigured = raw.passwordConfigured,
            passwordEnabled = raw.passwordEnabled
        };
    }

    public static void Save(WebUiSettingsData raw)
    {
        WebUiSettingsData data = Sanitize(raw);

        SaveLoadSettings.SaveString(SaveLoadSettings.DeviceNetworkKey, data.deviceName);
        SaveLoadSettings.SaveInt(SaveLoadSettings.FixtureModeKey, ToFixtureModeIndex(data.fixtureMode));
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxUniverseKey, data.dmxUniverse);
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxChannelKey, data.startChannel);
        SaveLoadSettings.SaveInt(SaveLoadSettings.FixtureCountKey, data.fixtureAmount);
        SaveLoadSettings.SaveInt(SaveLoadSettings.PixelColumnsKey, data.gridX);
        SaveLoadSettings.SaveInt(SaveLoadSettings.PixelRowsKey, data.gridY);
        SaveLoadSettings.SaveInt(SaveLoadSettings.NetworkModeKey, data.networkMode);
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnUseMulticastKey, data.useMulticast ? 1 : 0);
        SaveLoadSettings.SaveString(SaveLoadSettings.SAcnMulticastAddressKey, data.multicastAddress);
        SaveLoadSettings.SaveString(SaveLoadSettings.SAcnUnicastBindAddressKey, data.unicastBindAddress);
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnListenPortKey, data.listenPort);
        SaveLoadSettings.SaveFloat(SaveLoadSettings.SAcnTimeoutSecondsKey, data.timeoutSeconds);
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnUseLtpMergeKey, data.useLtpMerge ? 1 : 0);
        SaveLoadSettings.SaveString(SaveLoadSettings.SAcnMulticastUniversesKey, data.additionalUniverses);
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnDebugVisibleKey, data.showNetworkDebug ? 1 : 0);
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
