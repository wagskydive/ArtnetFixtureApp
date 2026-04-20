using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_FixtureModeSelector : MonoBehaviour
{
    [SerializeField] private Text modeValueText;
    [SerializeField] private GameObject pixelGridControlsContainer;
    [SerializeField] private Text pixelRowsValueText;
    [SerializeField] private Text pixelColumnsValueText;
    [SerializeField] private UI_FixtureMeshManager fixtureMeshManager;
    [SerializeField] private UI_InfoPanelController infoPanelController;
    [SerializeField] private GameObject fixtureCountControlsContainer;
    [SerializeField] private DmxModeManager dmxModeManager;
    private int currentPixelRows = PixelGridSnapshot.MinPixelWallSize;
    private int currentPixelColumns = PixelGridSnapshot.MinPixelWallSize;

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
        PixelGridService.Instance.OnChanged += HandlePixelGridSettingsChanged;
    }



    private void OnDisable()
    {
        SaveLoadSettings.OnFixtureModeSaved -= HandleFixtureModeSaved;
        PixelGridService.Instance.OnChanged -= HandlePixelGridSettingsChanged;

    }
    private void HandleFixtureModeSaved(FixtureMode fixtureMode)
    {
        UpdateDisplayOnSettingsSave();
        SyncUiState();
    }
    private void HandlePixelGridSettingsChanged(PixelGridSnapshot snapshot)
    {
        currentPixelRows = snapshot.Rows;
        currentPixelColumns = snapshot.Columns;
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
        int clamped = PixelGridSnapshot.ClampPixelDimension(currentPixelRows + PixelGridSnapshot.PixelWallStepSize);
        PixelGridService.Instance.Save(new PixelGridSnapshot(clamped, currentPixelColumns));

    }

    public void DecreasePixelRows()
    {
        int clamped = PixelGridSnapshot.ClampPixelDimension(currentPixelRows - PixelGridSnapshot.PixelWallStepSize);
        PixelGridService.Instance.Save(new PixelGridSnapshot(clamped, currentPixelColumns));
    }

    public void IncreasePixelColumns()
    {
        int clamped = PixelGridSnapshot.ClampPixelDimension(currentPixelColumns + PixelGridSnapshot.PixelWallStepSize);
        PixelGridService.Instance.Save(new PixelGridSnapshot(currentPixelRows, clamped));
    }

    public void DecreasePixelColumns()
    {
        int clamped = PixelGridSnapshot.ClampPixelDimension(currentPixelColumns - PixelGridSnapshot.PixelWallStepSize);
        PixelGridService.Instance.Save(new PixelGridSnapshot(currentPixelRows, clamped));
    }


    public void UpdateDisplayOnSettingsSave()
    {
        LoadPreferences();
        SyncUiState();
    }

    public void LoadPreferences()
    {
        dmxModeManager.SetFixtureMode((FixtureMode)Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.FixtureModeKey, (int)FixtureMode.Standard), 0, (int)FixtureMode.PixelMapping));
        PixelGridSnapshot pixelGrid = PixelGridService.Instance.CurrentPixelGrid;
        currentPixelRows = pixelGrid.Rows;
        currentPixelColumns = pixelGrid.Columns;
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
