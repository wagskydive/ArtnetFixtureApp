using UnityEngine;

public class WebUiSettingsBridge : MonoBehaviour
{
    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] private UI_FixtureMeshManager fixtureMeshManager;
    [SerializeField] private UI_FixtureModeSelector fixtureModeSelector;

    private void Start()
    {
        ApplySettings(WebUiSettingsStore.Load());
    }

    public WebUiSettingsData GetSettings()
    {
        return WebUiSettingsStore.Load();
    }

    public string GetSettingsJson()
    {
        return WebUiSettingsStore.ToJson(GetSettings());
    }

    public WebUiSettingsData SaveSettingsFromJson(string json)
    {
        WebUiSettingsData parsed = WebUiSettingsStore.FromJson(json);
        WebUiSettingsStore.Save(parsed);
        ApplySettings(parsed);
        return parsed;
    }

    public void ApplySettings(WebUiSettingsData raw)
    {
        WebUiSettingsData data = WebUiSettingsStore.Sanitize(raw);

        if (artNetReceiver != null)
        {
            artNetReceiver.SetUniverseFromUserInput(data.dmxUniverse);
            artNetReceiver.SetStartChannelFromUserInput(data.startChannel);
        }

        if (fixtureModeSelector != null)
        {
            fixtureModeSelector.SetMode(ToFixtureMode(data.fixtureMode));

            if (fixtureModeSelector.CurrentPixelColumns != data.gridX)
            {
                fixtureModeSelector.CurrentPixelColumns = data.gridX;
            }

            if (fixtureModeSelector.CurrentPixelRows != data.gridY)
            {
                fixtureModeSelector.CurrentPixelRows = data.gridY;
            }
        }

        if (fixtureMeshManager != null)
        {
            fixtureMeshManager.RebuildFixtures(data.fixtureAmount, savePreference: false);
            fixtureMeshManager.SyncFixtureAddresses();
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
}
