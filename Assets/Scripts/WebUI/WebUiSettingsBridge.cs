using UnityEngine;

public class WebUiSettingsBridge : MonoBehaviour
{
    [SerializeField] private UI_FixtureMeshManager fixtureMeshManager;
    [SerializeField] private UI_FixtureModeSelector fixtureModeSelector;
    [SerializeField] private CapabilityDefinition universeLimitCapability;


    public WebUiSettingsData GetSettings()
    {
        WebUiSettingsData settings = WebUiSettingsStore.Load();
        settings.advancedNetworkingUnlocked = IsAdvancedNetworkingUnlocked();
        settings.maxSelectableUniverse = GetMaxSelectableUniverse(universeLimitCapability);
        return settings;
    }

    public string GetSettingsJson()
    {
        return WebUiSettingsStore.ToJson(GetSettings());
    }

    public WebUiSettingsData SaveSettingsFromJson(string json)
    {
        WebUiSettingsData parsed = WebUiSettingsStore.FromJson(json);
        int maxSelectableUniverse = GetMaxSelectableUniverse(universeLimitCapability);
        parsed.maxSelectableUniverse = maxSelectableUniverse;
        parsed.dmxUniverse = Mathf.Clamp(parsed.dmxUniverse, 1, maxSelectableUniverse);
        WebUiSettingsStore.Save(parsed);
        return parsed;
    }

    

    private static FixtureMode ToFixtureMode(string fixtureMode)
    {
        if (fixtureMode == "moving")
        {
            return FixtureMode.MovingHead;
        }

        if (fixtureMode == "pixel")
        {
            return FixtureMode.PixelMapping;
        }

        return FixtureMode.Standard;
    }

    private static int GetMaxSelectableUniverse(CapabilityDefinition capabilityDefinition)
    {
        if (CapabilityService.Instance == null || capabilityDefinition == null || string.IsNullOrWhiteSpace(capabilityDefinition.Id))
        {
            return 1;
        }

        int maxUniverse = CapabilityService.Instance.ResolveNumeric(capabilityDefinition.Id, 1);
        return Mathf.Clamp(maxUniverse, 1, 63999);
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
