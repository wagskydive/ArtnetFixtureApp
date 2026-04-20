using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WebUiSettingsData
{
    public string serverSessionId = string.Empty;
    public string deviceName = "ArtnetFixture";
    public string ipAddress = "0.0.0.0";
    public string fixtureMode = "surface";
    public int dmxUniverse = 1;
    public int startChannel = 1;
    public int fixtureAmount = 1;
    public int gridX = 8;
    public int gridY = 8;
    public bool advancedNetworkingUnlocked;
    public int maxSelectableUniverse = 1;
    public bool isSAcnMode = false;
    public bool useMulticast = true;
    public string multicastAddress = "239.255.0.1";
    public string unicastBindAddress = "0.0.0.0";
    public int listenPort = 5568;
    public float timeoutSeconds = 2f;
    public bool useLtpMerge;
    public List<int> additionalUniverses = new List<int>();
    public bool showNetworkDebug;
    public bool passwordConfigured;
    public bool passwordEnabled;
    public string dmxInfoSurface = string.Empty;
    public string dmxInfoMovingHead = string.Empty;
    public string dmxInfoPixelMapping = string.Empty;
}

public static class WebUiSettingsStore
{
    public static WebUiSettingsData Load()
    {
        int fixtureMode = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.FixtureModeKey, 0), 0, 2);
        DmxSettingsSnapshot dmxSettingsSnapshot = DmxSettingsService.Instance.CurrentDmxSettings;
        return new WebUiSettingsData
        {
            serverSessionId = string.Empty,

            deviceName = SaveLoadSettings.LoadString(SaveLoadSettings.DeviceNetworkKey, "DMX Projector"),
            fixtureMode = ToFixtureModeValue(fixtureMode),
            dmxUniverse = dmxSettingsSnapshot.Universe1Based,
            startChannel = dmxSettingsSnapshot.StartChannel,
            fixtureAmount = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.FixtureCountKey, 1), 1, 16),
            gridX = PixelGridService.Instance.CurrentPixelGrid.Columns,
            gridY = PixelGridService.Instance.CurrentPixelGrid.Rows,
            isSAcnMode = dmxSettingsSnapshot.IsSAcnMode,

            useMulticast = dmxSettingsSnapshot.CurrentSAcnParameters.UseMulticast,
            multicastAddress = dmxSettingsSnapshot.CurrentSAcnParameters.MulticastAddress,
            unicastBindAddress = dmxSettingsSnapshot.CurrentSAcnParameters.UnicastBindAddress,
            listenPort = dmxSettingsSnapshot.CurrentSAcnParameters.ListenPort,
            timeoutSeconds = dmxSettingsSnapshot.CurrentSAcnParameters.TimeoutSeconds,
            useLtpMerge = dmxSettingsSnapshot.CurrentSAcnParameters.UseLtpMerge,
            additionalUniverses = new List<int>(dmxSettingsSnapshot.CurrentSAcnParameters.MulticastUniverseSubscriptions),
            showNetworkDebug = dmxSettingsSnapshot.CurrentSAcnParameters.DebugPanelVisible,
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
            serverSessionId = string.IsNullOrWhiteSpace(raw.serverSessionId) ? string.Empty : raw.serverSessionId.Trim(),
            deviceName = string.IsNullOrWhiteSpace(raw.deviceName) ? "ArtnetFixture" : raw.deviceName.Trim(),
            ipAddress = string.IsNullOrWhiteSpace(raw.ipAddress) ? "127.0.0.1" : raw.ipAddress.Trim(),
            fixtureMode = NormalizeFixtureMode(raw.fixtureMode),
            dmxUniverse = Mathf.Clamp(raw.dmxUniverse, 1, 63999),
            startChannel = Mathf.Clamp(raw.startChannel, 1, 512),
            fixtureAmount = Mathf.Clamp(raw.fixtureAmount, 1, 16),
            gridX = PixelGridSnapshot.ClampPixelDimension(raw.gridX),
            gridY = PixelGridSnapshot.ClampPixelDimension(raw.gridY),
            advancedNetworkingUnlocked = raw.advancedNetworkingUnlocked,
            maxSelectableUniverse = Mathf.Clamp(raw.maxSelectableUniverse, 1, 63999),
            isSAcnMode = raw.isSAcnMode,
            useMulticast = raw.useMulticast,
            multicastAddress = string.IsNullOrWhiteSpace(raw.multicastAddress) ? "239.255.0.1" : raw.multicastAddress.Trim(),
            unicastBindAddress = string.IsNullOrWhiteSpace(raw.unicastBindAddress) ? "0.0.0.0" : raw.unicastBindAddress.Trim(),
            listenPort = Mathf.Clamp(raw.listenPort, 1, 65535),
            timeoutSeconds = Mathf.Max(0.1f, raw.timeoutSeconds),
            useLtpMerge = raw.useLtpMerge,
            additionalUniverses = raw.additionalUniverses != null ? new List<int>(raw.additionalUniverses) : new List<int>(),
            showNetworkDebug = raw.showNetworkDebug,
            passwordConfigured = raw.passwordConfigured,
            passwordEnabled = raw.passwordEnabled,
            dmxInfoSurface = raw.dmxInfoSurface ?? string.Empty,
            dmxInfoMovingHead = raw.dmxInfoMovingHead ?? string.Empty,
            dmxInfoPixelMapping = raw.dmxInfoPixelMapping ?? string.Empty
        };
    }

    public static void Save(WebUiSettingsData raw)
    {
        WebUiSettingsData data = Sanitize(raw);

        //SaveLoadSettings.SaveString(SaveLoadSettings.DeviceNetworkKey, data.deviceName);
        SaveLoadSettings.SaveDeviceNetworkName(data.deviceName);

        //SaveLoadSettings.SaveInt(SaveLoadSettings.FixtureModeKey, ToFixtureModeIndex(data.fixtureMode));
        SaveLoadSettings.SaveFixtureMode(FixtureModeFromString(data.fixtureMode));

        //SaveLoadSettings.SaveInt(SaveLoadSettings.DmxUniverseKey, data.dmxUniverse);
        //SaveLoadSettings.SaveInt(SaveLoadSettings.DmxChannelKey, data.startChannel);
        //SaveLoadSettings.SaveInt(SaveLoadSettings.FixtureCountKey, data.fixtureAmount);
        SaveLoadSettings.SaveFixtureCount( data.fixtureAmount);

        //SaveLoadSettings.SaveInt(SaveLoadSettings.PixelColumnsKey, data.gridX);

        //SaveLoadSettings.SaveInt(SaveLoadSettings.PixelRowsKey, data.gridY);

        PixelGridService.Instance.Save(new PixelGridSnapshot(data.gridY, data.gridX));
        //SaveLoadSettings.SaveInt(SaveLoadSettings.NetworkModeKey, data.networkMode);
        //SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnUseMulticastKey, data.useMulticast ? 1 : 0);
        //SaveLoadSettings.SaveString(SaveLoadSettings.SAcnMulticastAddressKey, data.multicastAddress);
        //SaveLoadSettings.SaveString(SaveLoadSettings.SAcnUnicastBindAddressKey, data.unicastBindAddress);
        //SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnListenPortKey, data.listenPort);
        //SaveLoadSettings.SaveFloat(SaveLoadSettings.SAcnTimeoutSecondsKey, data.timeoutSeconds);
        //SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnUseLtpMergeKey, data.useLtpMerge ? 1 : 0);
        //SaveLoadSettings.SaveString(SaveLoadSettings.SAcnMulticastUniversesKey, data.additionalUniverses);
        //SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnDebugVisibleKey, data.showNetworkDebug ? 1 : 0);

        SAcnParameters sAcnParameters = new SAcnParameters(data.useMulticast,data.unicastBindAddress,data.listenPort,data.timeoutSeconds,data.useLtpMerge,data.additionalUniverses,data.showNetworkDebug);


        DmxSettingsSnapshot dmxSettingsSnapshot = new DmxSettingsSnapshot(data.dmxUniverse,data.startChannel,data.isSAcnMode,sAcnParameters);
        SaveLoadSettings.SaveDmxSettings(dmxSettingsSnapshot);
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

    private static FixtureMode FixtureModeFromString(string modeString)
    {
        if(modeString == "moving")
        {
            return FixtureMode.MovingHead;
        }
        if(modeString == "pixel")
        {
            return FixtureMode.PixelMapping;
        }
        return FixtureMode.Standard;
    }
}
