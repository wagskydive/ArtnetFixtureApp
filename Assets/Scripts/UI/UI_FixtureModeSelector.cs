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
        set
        {
            int clamped = Mathf.Clamp(value, MinPixelWallSize, MaxPixelWallSize);
            if (currentPixelRows == clamped)
            {
                return;
            }

            currentPixelRows = clamped;
            ApplyPixelGridSettings();
            SyncUiState();
            SavePreferences();
        }
    }

    public int CurrentPixelColumns
    {
        get => currentPixelColumns;
        set
        {
            int clamped = Mathf.Clamp(value, MinPixelWallSize, MaxPixelWallSize);
            if (currentPixelColumns == clamped)
            {
                return;
            }

            currentPixelColumns = clamped;
            ApplyPixelGridSettings();
            SyncUiState();
            SavePreferences();
        }
    }

    private void Start()
    {
        LoadPreferences();
        EnforceFixtureCountForMode();
        
        ApplyPixelGridSettings();
        SyncUiState();
        SaveLoadSettings.OnSettingsSaved += UpdateDisplayOnSettingsSave;
        dmxModeManager.ApplyModeMaterials();
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

    public void SetMode(DmxModeManager.FixtureMode mode)
    {
        if (dmxModeManager.CurrentMode == mode)
        {
            return;
        }

        dmxModeManager.SetFixtureMode(mode);
        EnforceFixtureCountForMode();

        ApplyPixelGridSettings();
        SyncUiState();
        SavePreferences();
        
    }

    public void IncreaseMode()
    {
        int modeCount = System.Enum.GetValues(typeof(DmxModeManager.FixtureMode)).Length;
        int nextMode = ((int)dmxModeManager.CurrentMode + 1) % modeCount;
        SetMode((DmxModeManager.FixtureMode)nextMode);

    }

    public void DecreaseMode()
    {
        int modeCount = System.Enum.GetValues(typeof(DmxModeManager.FixtureMode)).Length;
        int previousMode = ((int)dmxModeManager.CurrentMode - 1 + modeCount) % modeCount;
        SetMode((DmxModeManager.FixtureMode)previousMode);

    }

    public void IncreasePixelRows()
    {
        CurrentPixelRows = currentPixelRows + PixelWallStepSize;
    }

    public void DecreasePixelRows()
    {
        CurrentPixelRows = currentPixelRows - PixelWallStepSize;
    }

    public void IncreasePixelColumns()
    {
        CurrentPixelColumns = currentPixelColumns + PixelWallStepSize;
    }

    public void DecreasePixelColumns()
    {
        CurrentPixelColumns = currentPixelColumns - PixelWallStepSize;
    }

    public void SavePreferences()
    {
        SaveLoadSettings.SaveInt(SaveLoadSettings.FixtureModeKey, (int)dmxModeManager.CurrentMode);
        SaveLoadSettings.SaveInt(SaveLoadSettings.PixelRowsKey, currentPixelRows);
        SaveLoadSettings.SaveInt(SaveLoadSettings.PixelColumnsKey, currentPixelColumns);
        SaveLoadSettings.Save();
    }

    public void UpdateDisplayOnSettingsSave()
    {
        LoadPreferences();
        SyncUiState();
    }

    public void LoadPreferences()
    {
        dmxModeManager.SetFixtureMode((DmxModeManager.FixtureMode)Mathf.Clamp(SaveLoadSettings.LoadInt(SaveLoadSettings.FixtureModeKey, (int)DmxModeManager.FixtureMode.Standard), 0, (int)DmxModeManager.FixtureMode.PixelMapping));
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
            pixelGridControlsContainer.SetActive(dmxModeManager.CurrentMode == DmxModeManager.FixtureMode.PixelMapping);
        }

        if (fixtureCountControlsContainer != null)
        {
            fixtureCountControlsContainer.SetActive(dmxModeManager.CurrentMode == DmxModeManager.FixtureMode.Standard);
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

    private static string GetModeDisplayName(DmxModeManager.FixtureMode mode)
    {
        if (mode == DmxModeManager.FixtureMode.MovingHead)
        {
            return "Moving Head";
        }

        if (mode == DmxModeManager.FixtureMode.PixelMapping)
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

        if (dmxModeManager.CurrentMode == DmxModeManager.FixtureMode.Standard)
        {
            fixtureMeshManager.RestoreSavedFixtureCount();
            return;
        }

        if (fixtureMeshManager.FixtureCount != 1)
        {
            fixtureMeshManager.RebuildFixtures(1, savePreference: false);
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
