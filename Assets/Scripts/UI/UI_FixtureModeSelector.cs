using UnityEngine;
using UnityEngine.UI;

public class UI_FixtureModeSelector : MonoBehaviour
{
    private const int MinPixelWallSize = 8;
    private const int MaxPixelWallSize = 32;
    private const int PixelWallStepSize = 8;

    public enum FixtureMode
    {
        Standard = 0,
        MovingHead = 1,
        PixelMapping = 2
    }

    private const string FixtureModePrefKey = "dmx.fixture.mode";
    private const string PixelRowsPrefKey = "dmx.pixel.rows";
    private const string PixelColumnsPrefKey = "dmx.pixel.columns";

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material standardModeMaterial;
    [SerializeField] private Material movingHeadModeMaterial;
    [SerializeField] private Material pixelMappingModeMaterial;
    [SerializeField] private Text modeValueText;
    [SerializeField] private GameObject pixelGridControlsContainer;
    [SerializeField] private Text pixelRowsValueText;
    [SerializeField] private Text pixelColumnsValueText;
    [SerializeField] private UI_FixtureMeshManager fixtureMeshManager;
    [SerializeField] private GameObject fixtureCountControlsContainer;
    [SerializeField] private FixtureMode currentMode = FixtureMode.Standard;
    [SerializeField] private int currentPixelRows = 8;
    [SerializeField] private int currentPixelColumns = 8;

    public FixtureMode CurrentMode => currentMode;
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
        ApplyModeMaterials();
        ApplyPixelGridSettings();
        SyncUiState();
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

    public void SetMode(FixtureMode mode)
    {
        if (currentMode == mode)
        {
            return;
        }

        currentMode = mode;
        EnforceFixtureCountForMode();
        ApplyModeMaterials();
        ApplyPixelGridSettings();
        SyncUiState();
        SavePreferences();
    }

    public void IncreaseMode()
    {
        int modeCount = System.Enum.GetValues(typeof(FixtureMode)).Length;
        int nextMode = ((int)currentMode + 1) % modeCount;
        SetMode((FixtureMode)nextMode);
    }

    public void DecreaseMode()
    {
        int modeCount = System.Enum.GetValues(typeof(FixtureMode)).Length;
        int previousMode = ((int)currentMode - 1 + modeCount) % modeCount;
        SetMode((FixtureMode)previousMode);
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
        PlayerPrefs.SetInt(FixtureModePrefKey, (int)currentMode);
        PlayerPrefs.SetInt(PixelRowsPrefKey, currentPixelRows);
        PlayerPrefs.SetInt(PixelColumnsPrefKey, currentPixelColumns);
        PlayerPrefs.Save();
    }

    public void LoadPreferences()
    {
        currentMode = (FixtureMode)Mathf.Clamp(PlayerPrefs.GetInt(FixtureModePrefKey, (int)FixtureMode.Standard), 0, (int)FixtureMode.PixelMapping);
        currentPixelRows = Mathf.Clamp(PlayerPrefs.GetInt(PixelRowsPrefKey, currentPixelRows), MinPixelWallSize, MaxPixelWallSize);
        currentPixelColumns = Mathf.Clamp(PlayerPrefs.GetInt(PixelColumnsPrefKey, currentPixelColumns), MinPixelWallSize, MaxPixelWallSize);
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

        if (currentMode == FixtureMode.PixelMapping && pixelMappingModeMaterial != null)
        {
            targetRenderer.sharedMaterial = pixelMappingModeMaterial;
            return;
        }

        if (standardModeMaterial != null)
        {
            targetRenderer.sharedMaterial = standardModeMaterial;
        }
    }

    private void SyncUiState()
    {
        if (modeValueText != null)
        {
            modeValueText.text = GetModeDisplayName(currentMode);
        }

        if (pixelGridControlsContainer != null)
        {
            pixelGridControlsContainer.SetActive(currentMode == FixtureMode.PixelMapping);
        }

        if (fixtureCountControlsContainer != null)
        {
            fixtureCountControlsContainer.SetActive(currentMode == FixtureMode.Standard);
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

        return "Standard";
    }

    private void EnforceFixtureCountForMode()
    {
        if (fixtureMeshManager == null)
        {
            return;
        }

        if (currentMode != FixtureMode.Standard)
        {
            fixtureMeshManager.RebuildFixtures(1);
        }
    }

    private void ApplyPixelGridSettings()
    {
        if (pixelMappingModeMaterial != null)
        {
            pixelMappingModeMaterial.SetFloat("_Rows", currentPixelRows);
            pixelMappingModeMaterial.SetFloat("_Columns", currentPixelColumns);
        }

        if (targetRenderer == null || targetRenderer.sharedMaterial == null)
        {
            return;
        }

        targetRenderer.sharedMaterial.SetFloat("_Rows", currentPixelRows);
        targetRenderer.sharedMaterial.SetFloat("_Columns", currentPixelColumns);
    }
}
