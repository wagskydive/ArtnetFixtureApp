using UnityEngine;
using System;
public enum FixtureMode
{
    Standard = 0,
    MovingHead = 1,
    PixelMapping = 2
}
[DefaultExecutionOrder(-100)]
public class DmxModeManager : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material standardModeMaterial;
    [SerializeField] private Material movingHeadModeMaterial;
    [SerializeField] private Material pixelMappingModeMaterial;

    public Renderer TargetRenderer { get => targetRenderer; }

    public Material StandardModeMaterial { get => standardModeMaterial; }
    public Material MovingHeadMaterial { get => movingHeadModeMaterial; }
    public Material PixelMappingModeMaterial { get => pixelMappingModeMaterial; }

    public static event Action<FixtureMode> OnModeChanged;

    public static event Action Awoken;
    public static event Action OnManagerReady;



    public static DmxModeManager Instance { get; private set; }

    private void Awake()
    {
        // Check if an instance already exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy the duplicate
            return;
        }

        Instance = this;
        OnManagerReady?.Invoke();

    }

    void Start()
    {
        if (Instance != null && Instance == this)
        {
            Awoken?.Invoke();
        }
    }

    void OnEnable()
    {
        SaveLoadSettings.OnFixtureModeSaved += HandleModeSaved;
    }

    private void HandleModeSaved(FixtureMode mode)
    {
        //FixtureMode mode = (FixtureMode)SaveLoadSettings.LoadInt(SaveLoadSettings.FixtureModeKey, 0);
        SetFixtureMode(mode);
    }

    void OnDisable()
    {
        SaveLoadSettings.OnFixtureModeSaved -= HandleModeSaved;
    }
    private FixtureMode currentMode;

    public FixtureMode CurrentMode { get => currentMode; }

    public void SetFixtureMode(FixtureMode mode)
    {
        if (currentMode == mode)
        {
            return;
        }

        currentMode = mode;
        ApplyModeMaterials();
        OnModeChanged?.Invoke(mode);
    }

    public void ApplyModeMaterials()
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
}
