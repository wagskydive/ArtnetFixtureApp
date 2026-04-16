using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_FixtureModeSelector : MonoBehaviour
{
    private const int MinPixelWallSize = 8;
    private const int MaxPixelWallSize = 32;
    private const int PixelWallStepSize = 8;


    [SerializeField] private Text modeValueText;
    [SerializeField] private GameObject pixelGridControlsContainer;
    [SerializeField] private Text pixelRowsValueText;
    [SerializeField] private Text pixelColumnsValueText;
    [SerializeField] private UI_FixtureMeshManager fixtureMeshManager;
    [SerializeField] private UI_InfoPanelController infoPanelController;
    [SerializeField] private GameObject fixtureCountControlsContainer;
    [SerializeField] private DmxModeManager dmxModeManager;
    [SerializeField] private int currentPixelRows = 8;
    [SerializeField] private int currentPixelColumns = 8;

    public int CurrentPixelRows
    {
        get => currentPixelRows;
    }

    public int CurrentPixelColumns
    {
        get => currentPixelColumns;

    }

    private void Start()
    {
        LoadPreferences();
        EnforceFixtureCountForMode();

        ApplyPixelGridSettings();
        SyncUiState();

        dmxModeManager.ApplyModeMaterials();
    }

    void OnEnable()
    {
        SaveLoadSettings.OnFixtureModeSaved += HandleFixtureModeSaved;
        SaveLoadSettings.OnPixelGridSettingsSaved += HandlePixelGridSettingsSaved;
    }



    private void OnDisable()
    {
        SaveLoadSettings.OnFixtureModeSaved -= HandleFixtureModeSaved;
        SaveLoadSettings.OnPixelGridSettingsSaved -= HandlePixelGridSettingsSaved;

    }
    private void HandleFixtureModeSaved(FixtureMode fixtureMode)
    {
        UpdateDisplayOnSettingsSave();
        SyncUiState();
    }
    private void HandlePixelGridSettingsSaved(PixelGridSettings settings)
    {
        ApplyPixelGridSettings();
        SyncUiState();
    }



    public void SetMode(FixtureMode mode)
    {
        if (dmxModeManager.CurrentMode == mode)
        {
            return;
        }

        SaveLoadSettings.SaveFixtureMode(mode);
        EnforceFixtureCountForMode();

        ApplyPixelGridSettings();
        SyncUiState();
    }

    public void IncreaseMode()
    {
        int modeCount = System.Enum.GetValues(typeof(FixtureMode)).Length;
        int nextMode = ((int)dmxModeManager.CurrentMode + 1) % modeCount;

        SetMode((FixtureMode)nextMode);

    }

    public void DecreaseMode()
    {
        int modeCount = System.Enum.GetValues(typeof(FixtureMode)).Length;
        int previousMode = ((int)dmxModeManager.CurrentMode - 1 + modeCount) % modeCount;
        SetMode((FixtureMode)previousMode);

    }

    public void IncreasePixelRows()
    {
        int clamped = Mathf.Clamp(currentPixelRows + PixelWallStepSize, MinPixelWallSize, MaxPixelWallSize);
        SaveLoadSettings.SavePixelGridSettings(new PixelGridSettings(clamped, currentPixelColumns));

    }

    public void DecreasePixelRows()
    {
        int clamped = Mathf.Clamp(currentPixelRows - PixelWallStepSize, MinPixelWallSize, MaxPixelWallSize);
        SaveLoadSettings.SavePixelGridSettings(new PixelGridSettings(clamped, currentPixelColumns));
    }

    public void IncreasePixelColumns()
    {
        int clamped = Mathf.Clamp(currentPixelColumns + PixelWallStepSize, MinPixelWallSize, MaxPixelWallSize);
        SaveLoadSettings.SavePixelGridSettings(new PixelGridSettings(currentPixelRows, clamped));
    }

    public void DecreasePixelColumns()
    {
        int clamped = Mathf.Clamp(currentPixelColumns - PixelWallStepSize, MinPixelWallSize, MaxPixelWallSize);
        SaveLoadSettings.SavePixelGridSettings(new PixelGridSettings(currentPixelRows, clamped));
    }


    public void UpdateDisplayOnSettingsSave()
    {
        LoadPreferences();
        SyncUiState();
    }

    public void LoadPreferences()
    {
        dmxModeManager.SetFixtureMode((FixtureMode)Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.FixtureModeKey, (int)FixtureMode.Standard), 0, (int)FixtureMode.PixelMapping));
        currentPixelRows = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.PixelRowsKey, currentPixelRows), MinPixelWallSize, MaxPixelWallSize);
        currentPixelColumns = Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.PixelColumnsKey, currentPixelColumns), MinPixelWallSize, MaxPixelWallSize);
    }

    private void SyncUiState()
    {
        if (modeValueText != null)
        {
            modeValueText.text = GetModeDisplayName(dmxModeManager.CurrentMode);
        }

        if (pixelGridControlsContainer != null)
        {
            pixelGridControlsContainer.SetActive(dmxModeManager.CurrentMode == FixtureMode.PixelMapping);
        }

        if (fixtureCountControlsContainer != null)
        {
            fixtureCountControlsContainer.SetActive(dmxModeManager.CurrentMode == FixtureMode.Standard);
        }

        if (pixelRowsValueText != null)
        {
            pixelRowsValueText.text = currentPixelRows.ToString();
        }

        if (pixelColumnsValueText != null)
        {
            pixelColumnsValueText.text = currentPixelColumns.ToString();
        }
    }

    private static string GetModeDisplayName(FixtureMode mode)
    {
        if (mode == FixtureMode.MovingHead)
        {
            return "Moving Head";
        }

        if (mode == FixtureMode.PixelMapping)
        {
            return "Pixel Mapping";
        }

        return "Surface";
    }

    private void EnforceFixtureCountForMode()
    {
        if (fixtureMeshManager == null)
        {
            return;
        }

        if (dmxModeManager.CurrentMode == FixtureMode.Standard)
        {
            fixtureMeshManager.RestoreSavedFixtureCount();
            return;
        }

        if (fixtureMeshManager.FixtureCount != 1)
        {
            fixtureMeshManager.RebuildFixtures(1);
        }
    }

    private void ApplyPixelGridSettings()
    {

        if (dmxModeManager.PixelMappingModeMaterial != null)
        {
            dmxModeManager.PixelMappingModeMaterial.SetFloat("_Rows", currentPixelRows);
            dmxModeManager.PixelMappingModeMaterial.SetFloat("_Columns", currentPixelColumns);
        }

        if (dmxModeManager.TargetRenderer == null || dmxModeManager.TargetRenderer.sharedMaterial == null)
        {
            return;
        }

        dmxModeManager.TargetRenderer.sharedMaterial.SetFloat("_Rows", currentPixelRows);
        dmxModeManager.TargetRenderer.sharedMaterial.SetFloat("_Columns", currentPixelColumns);
    }
}
