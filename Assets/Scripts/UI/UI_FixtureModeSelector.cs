using UnityEngine;
using UnityEngine.UI;

public class UI_FixtureModeSelector : MonoBehaviour
{
    private const int MinPixelWallSize = 1;
    private const int MaxPixelWallSize = 32;

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
    [SerializeField] private Dropdown modeDropdown;
    [SerializeField] private Text pixelRowsValueText;
    [SerializeField] private Text pixelColumnsValueText;
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
            SyncPixelGridUi();
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
            SyncPixelGridUi();
            SavePreferences();
        }
    }

    private void Start()
    {
        LoadPreferences();
        ApplyModeMaterials();
        ApplyPixelGridSettings();
        SyncDropdown();
        SyncPixelGridUi();
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
        SetMode((FixtureMode)Mathf.Clamp(modeIndex, 0, (int)FixtureMode.PixelMapping));
    }

    public void SetMode(FixtureMode mode)
    {
        if (currentMode == mode)
        {
            return;
        }

        currentMode = mode;
        ApplyModeMaterials();
        ApplyPixelGridSettings();
        SyncDropdown();
        SavePreferences();
    }

    public void IncreasePixelRows()
    {
        CurrentPixelRows = currentPixelRows + 1;
    }

    public void DecreasePixelRows()
    {
        CurrentPixelRows = currentPixelRows - 1;
    }

    public void IncreasePixelColumns()
    {
        CurrentPixelColumns = currentPixelColumns + 1;
    }

    public void DecreasePixelColumns()
    {
        CurrentPixelColumns = currentPixelColumns - 1;
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

    private void SyncDropdown()
    {
        if (modeDropdown == null)
        {
            return;
        }

        modeDropdown.SetValueWithoutNotify((int)currentMode);
    }

    private void SyncPixelGridUi()
    {
        if (pixelRowsValueText != null)
        {
            pixelRowsValueText.text = currentPixelRows.ToString();
        }

        if (pixelColumnsValueText != null)
        {
            pixelColumnsValueText.text = currentPixelColumns.ToString();
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
