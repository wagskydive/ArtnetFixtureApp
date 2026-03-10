using UnityEngine;
using UnityEngine.UI;

public class UI_FixtureModeSelector : MonoBehaviour
{
    public enum FixtureMode
    {
        Standard = 0,
        MovingHead = 1
    }

    private const string FixtureModePrefKey = "dmx.fixture.mode";

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material standardModeMaterial;
    [SerializeField] private Material movingHeadModeMaterial;
    [SerializeField] private Dropdown modeDropdown;
    [SerializeField] private FixtureMode currentMode = FixtureMode.Standard;

    public FixtureMode CurrentMode => currentMode;

    private void Start()
    {
        LoadPreferences();
        ApplyModeMaterials();
        SyncDropdown();
    }

    private void OnDisable()
    {
        SavePreferences();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SavePreferences();
        }
    }

    public void SetModeFromDropdown(int modeIndex)
    {
        SetMode((FixtureMode)Mathf.Clamp(modeIndex, 0, 1));
    }

    public void SetMode(FixtureMode mode)
    {
        if (currentMode == mode)
        {
            return;
        }

        currentMode = mode;
        ApplyModeMaterials();
        SyncDropdown();
        SavePreferences();
    }

    public void SavePreferences()
    {
        PlayerPrefs.SetInt(FixtureModePrefKey, (int)currentMode);
        PlayerPrefs.Save();
    }

    public void LoadPreferences()
    {
        currentMode = (FixtureMode)Mathf.Clamp(PlayerPrefs.GetInt(FixtureModePrefKey, (int)FixtureMode.Standard), 0, 1);
    }

    private void ApplyModeMaterials()
    {
        if (targetRenderer == null)
        {
            return;
        }

        if (currentMode == FixtureMode.MovingHead && movingHeadModeMaterial != null)
        {
            targetRenderer.sharedMaterial = movingHeadModeMaterial;
            return;
        }

        if (standardModeMaterial != null)
        {
            targetRenderer.sharedMaterial = standardModeMaterial;
        }
    }

    private void SyncDropdown()
    {
        if (modeDropdown == null)
        {
            return;
        }

        modeDropdown.SetValueWithoutNotify((int)currentMode);
    }
}
