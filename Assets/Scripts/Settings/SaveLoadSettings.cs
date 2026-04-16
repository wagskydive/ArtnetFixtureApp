using UnityEngine;
using System;

public readonly struct PixelGridSettings
{
    public readonly int PixelRows;
    public readonly int PixelColumns;

    public PixelGridSettings(int rows, int columns)
    {
        PixelRows = rows;
        PixelColumns = columns;
    }

}



public static class SaveLoadSettings
{

    // DMX Settings
    public const string DmxUniverseKey = "dmx.universe";
    public const string DmxChannelKey = "dmx.channel";
    public const string NetworkModeKey = "network.mode";


    // Mode Specific Settings

    public const string FixtureModeKey = "dmx.fixture.mode";

    public const string FixtureCountKey = "dmx.fixture.count";

    public const string PixelRowsKey = "dmx.pixel.rows";
    public const string PixelColumnsKey = "dmx.pixel.columns";

    // Network Name
    public const string DeviceNetworkKey = "device.network.name";


    public const string WebUiPasswordKey = "webui.password";
    public const string WebUiPasswordEnabledKey = "webui.password.enabled";


    // SAcnParameter Settings
    public const string SAcnUseMulticastKey = "sacn.use.multicast";
    public const string SAcnMulticastAddressKey = "sacn.multicast.address";
    public const string SAcnUnicastBindAddressKey = "sacn.unicast.bind.address";
    public const string SAcnListenPortKey = "sacn.listen.port";
    public const string SAcnTimeoutSecondsKey = "sacn.timeout.seconds";
    public const string SAcnUseLtpMergeKey = "sacn.use.ltp.merge";
    public const string SAcnMulticastUniversesKey = "sacn.multicast.universes";
    public const string SAcnDebugVisibleKey = "sacn.debug.visible";

    //UI Settings
    public const string NetworkWarningEnabledKey = "network.warning.enabled";
    public const string InfoPanelEnabledKey = "info.panel.enabled";

    // Entitlement Settings
    public const string IapEntitlementsKey = "iap.entitlements";
    public const string IapConsumablesKey = "iap.consumables";

    public const string LastValidationUnixKey = "iap_last_validation_unix";
    public const string FallbackDeviceIdKey = "iap_device_id";


    public static event Action OnAnySettingsSaved;
    public static event Action<SAcnParameters> OnSAcnParametersSaved;
    public static event Action<PixelGridSettings> OnPixelGridSettingsSaved;
    public static event Action<FixtureMode> OnFixtureModeSaved;
    public static event Action<string> OnDeviceNetworkNameSaved;
    public static event Action<int> OnFixtureCountSaved;
    public static event Action<bool> OnNetworkWarningBannerEnabledSaved;
    public static event Action<bool> OnInfoPanelEnabledSaved;

    public static event Action<string> OnEntitlementsSaved;
    public static event Action<string> OnConsumablesSaved;

    public static event Action<string> OnWebUiPasswordSaved;

    public static event Action<bool> OnWebUiPasswordEnabledSaved;

    public static int LoadInt(string key, int defaultValue)
    {
        int result = PlayerPrefs.GetInt(key, defaultValue);
        return result;
    }

    public static string LoadString(string key, string defaultValue)
    {
        string result = PlayerPrefs.GetString(key, defaultValue);
        return result;
    }

    public static float LoadFloat(string key, float defaultValue)
    {
        float result = PlayerPrefs.GetFloat(key, defaultValue);
        return result;
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

    private static void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    private static void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }

    private static void SaveLong(string key, long value)
    {
        PlayerPrefs.SetString(key, value.ToString());
    }

    private static void SaveFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }

    private static void SaveAndInvokeEvent()
    {
        PlayerPrefs.Save();
        OnAnySettingsSaved?.Invoke();
    }

    public static void SaveDmxSettings(DmxSettingsSnapshot snapshot)
    {

        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxUniverseKey, snapshot.Universe1Based);
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxChannelKey, snapshot.StartChannel);

        SaveLoadSettings.SaveInt(SaveLoadSettings.NetworkModeKey, snapshot.IsSAcnMode ? 1 : 0);


        SaveSAcnParameters(snapshot.CurrentSAcnParameters);
        SaveLoadSettings.SaveAndInvokeEvent();

        DmxSettingsBus.Publish(snapshot);
    }


    public static void SaveSAcnParameters(SAcnParameters p)

    {
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnUseMulticastKey, p.UseMulticast ? 1 : 0);
        SaveLoadSettings.SaveString(SaveLoadSettings.SAcnMulticastAddressKey, p.MulticastAddress);
        SaveLoadSettings.SaveString(SaveLoadSettings.SAcnUnicastBindAddressKey, p.UnicastBindAddress);
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnListenPortKey, p.ListenPort);
        SaveLoadSettings.SaveFloat(SaveLoadSettings.SAcnTimeoutSecondsKey, p.TimeoutSeconds);
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnUseLtpMergeKey, p.UseLtpMerge ? 1 : 0);
        SaveLoadSettings.SaveString(SaveLoadSettings.SAcnMulticastUniversesKey, SAcnParameters.BuildUniverseListCsv(p.MulticastUniverseSubscriptions));
        SaveLoadSettings.SaveInt(SaveLoadSettings.SAcnDebugVisibleKey, p.DebugPanelVisible ? 1 : 0);
        SaveAndInvokeEvent();
        OnSAcnParametersSaved?.Invoke(p);
    }

    public static void SavePixelGridSettings(PixelGridSettings pixelGridSettings)
    {
        SaveInt(PixelRowsKey, pixelGridSettings.PixelRows);
        SaveInt(PixelColumnsKey, pixelGridSettings.PixelColumns);
        SaveAndInvokeEvent();
        OnPixelGridSettingsSaved?.Invoke(pixelGridSettings);
    }

    public static void SaveFixtureMode(FixtureMode fixtureMode)
    {
        SaveInt(FixtureModeKey, (int)fixtureMode);
        SaveAndInvokeEvent();
        OnFixtureModeSaved?.Invoke(fixtureMode);
    }

    public static void SaveFixtureCount(int count)
    {
        SaveInt(FixtureCountKey, count);
        SaveAndInvokeEvent();
        OnFixtureCountSaved?.Invoke(count);
    }

    public static void SaveDeviceNetworkName(string name)
    {
        SaveString(DeviceNetworkKey, name);
        SaveAndInvokeEvent();
        OnDeviceNetworkNameSaved?.Invoke(name);
    }

    public static void SaveNetworkWarningBannerEnabled(bool isEnabled)
    {
        SaveInt(NetworkWarningEnabledKey, isEnabled ? 1 : 0);
        SaveAndInvokeEvent();
        OnNetworkWarningBannerEnabledSaved?.Invoke(isEnabled);
    }

    public static void SaveInfoPanelEnabled(bool isEnabled)
    {
        SaveInt(InfoPanelEnabledKey, isEnabled ? 1 : 0);
        SaveAndInvokeEvent();
        OnInfoPanelEnabledSaved?.Invoke(isEnabled);
    }

    public static void SaveEntitlements(string encryptedEntitlements)
    {
        SaveString(IapEntitlementsKey, encryptedEntitlements);
        SaveAndInvokeEvent();
        OnEntitlementsSaved?.Invoke(encryptedEntitlements);
    }

    public static void SaveConsumables(string consumables)
    {
        SaveString(IapConsumablesKey, consumables);
        SaveAndInvokeEvent();
        OnConsumablesSaved?.Invoke(consumables);
    }

    public static void SaveWebUiPassword(string password)
    {
        SaveString(WebUiPasswordKey, password);
        SaveAndInvokeEvent();
        OnWebUiPasswordSaved?.Invoke(password);
    }

    public static void SaveWebUiPasswordEnabled(bool isEnabled)
    {
        SaveInt(WebUiPasswordEnabledKey, isEnabled ? 1 : 0);
        SaveAndInvokeEvent();
        OnWebUiPasswordEnabledSaved?.Invoke(isEnabled);
    }

    public static void SaveLastValidationUnix(long last)
    {
        SaveLong(LastValidationUnixKey, last);
        SaveAndInvokeEvent();

    }

    public static void SaveFallbackDeviceId(string id)
    {
        SaveString(FallbackDeviceIdKey,id);
        SaveAndInvokeEvent();
    }

}

