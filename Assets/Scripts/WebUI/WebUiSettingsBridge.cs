using UnityEngine;

public class WebUiSettingsBridge : MonoBehaviour
{
        [SerializeField] private UI_FixtureMeshManager fixtureMeshManager;
    [SerializeField] private UI_FixtureModeSelector fixtureModeSelector;
    [SerializeField] private CapabilityDefinition universeLimitCapability;

    private void Start()
    {
        ApplySettings(WebUiSettingsStore.Load());
    }

    public WebUiSettingsData GetSettings()
    {
        WebUiSettingsData settings = WebUiSettingsStore.Load();
        settings.advancedNetworkingUnlocked = IsAdvancedNetworkingUnlocked();
        return settings;
    }

    public string GetSettingsJson()
    {
        return WebUiSettingsStore.ToJson(GetSettings());
    }

    public WebUiSettingsData SaveSettingsFromJson(string json)
    {
        WebUiSettingsData parsed = WebUiSettingsStore.FromJson(json);
        parsed.dmxUniverse = Mathf.Clamp(parsed.dmxUniverse, 1, GetMaxSelectableUniverse(universeLimitCapability));
        WebUiSettingsStore.Save(parsed);
        ApplySettings(parsed);
        return parsed;
    }

    public void ApplySettings(WebUiSettingsData raw)
    {
        WebUiSettingsData data = WebUiSettingsStore.Sanitize(raw);
        data.dmxUniverse = Mathf.Clamp(data.dmxUniverse, 1, GetMaxSelectableUniverse(universeLimitCapability));
        bool advancedUnlocked = IsAdvancedNetworkingUnlocked();
        data.advancedNetworkingUnlocked = advancedUnlocked;
        DmxModeManager.FixtureMode selectedMode = ToFixtureMode(data.fixtureMode);

        if (fixtureModeSelector != null)
        {
            fixtureModeSelector.SetMode(selectedMode);

            if (fixtureModeSelector.CurrentPixelColumns != data.gridX)
            {
                fixtureModeSelector.CurrentPixelColumns = data.gridX;
            }

            if (fixtureModeSelector.CurrentPixelRows != data.gridY)
            {
                fixtureModeSelector.CurrentPixelRows = data.gridY;
            }
        }

        int fixtureCount = selectedMode == DmxModeManager.FixtureMode.Standard ? data.fixtureAmount : 1;

        if (fixtureMeshManager != null)
        {
            fixtureMeshManager.RebuildFixtures(fixtureCount, savePreference: false);
            fixtureMeshManager.SetPrimaryReceiverAddressFromUserInput(data.dmxUniverse, data.startChannel);
            fixtureMeshManager.SyncFixtureAddresses();
            return;
        }

        INetworkReceiver receiver = NetworkingModeManager.Instance?.NetworkReceiver;
        if (advancedUnlocked && NetworkingModeManager.Instance != null)
        {
            NetworkingModeManager.Instance.SetModeFromIndex(data.networkMode);
            receiver = NetworkingModeManager.Instance.NetworkReceiver;
        }

        if (receiver != null)
        {
            receiver.SetUniverseFromUserInput(data.dmxUniverse);
            receiver.SetStartChannelFromUserInput(data.startChannel);
        }

        if (advancedUnlocked && receiver is SAcnReceiver sacnReceiver)
        {
            sacnReceiver.SetTransportMode(data.useMulticast);
            sacnReceiver.SetMulticastAddressFromUserInput(data.multicastAddress);
            sacnReceiver.SetUnicastBindAddressFromUserInput(data.unicastBindAddress);
            sacnReceiver.SetListenPortFromUserInput(data.listenPort);
            sacnReceiver.TimeoutSeconds = Mathf.Max(0.1f, data.timeoutSeconds);
            sacnReceiver.UseLtpMerge = data.useLtpMerge;
            sacnReceiver.MulticastUniverseSubscriptions = ParseUniverseCsv(data.additionalUniverses);
            sacnReceiver.Parameters.debugPanelVisible = data.showNetworkDebug;
            sacnReceiver.SaveNetworkSettings();
        }

        if (advancedUnlocked && NetworkDebugService.Instance != null)
        {
            NetworkDebugService.Instance.DebugVisible = data.showNetworkDebug;
        }
    }

    private static DmxModeManager.FixtureMode ToFixtureMode(string fixtureMode)
    {
        if (fixtureMode == "moving")
        {
            return DmxModeManager.FixtureMode.MovingHead;
        }

        if (fixtureMode == "pixel")
        {
            return DmxModeManager.FixtureMode.PixelMapping;
        }

        return DmxModeManager.FixtureMode.Standard;
    }

    private static int GetMaxSelectableUniverse(CapabilityDefinition capabilityDefinition)
    {
        if (CapabilityService.Instance == null || capabilityDefinition == null || string.IsNullOrWhiteSpace(capabilityDefinition.Id))
        {
            return 1;
        }

        int maxUniverse = CapabilityService.Instance.ResolveNumeric(capabilityDefinition.Id, 1);
        return Mathf.Clamp(maxUniverse, 1, 16);
    }

    private static bool IsAdvancedNetworkingUnlocked()
    {
        return CapabilityService.Instance != null
            && CapabilityService.Instance.ResolveBoolean("capability.advanced.networking", false);
    }

    private static System.Collections.Generic.List<int> ParseUniverseCsv(string csv)
    {
        var values = new System.Collections.Generic.List<int>();
        if (string.IsNullOrWhiteSpace(csv))
        {
            return values;
        }

        string[] entries = csv.Split(',');
        for (int i = 0; i < entries.Length; i++)
        {
            if (!int.TryParse(entries[i].Trim(), out int universe1Based))
            {
                continue;
            }

            int value = Mathf.Clamp(universe1Based, 1, 64000) - 1;
            if (!values.Contains(value))
            {
                values.Add(value);
            }
        }

        return values;
    }
}
