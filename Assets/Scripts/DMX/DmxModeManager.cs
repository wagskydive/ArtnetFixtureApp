using UnityEngine;
using System;


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

    public enum FixtureMode
    {
        Standard = 0,
        MovingHead = 1,
        PixelMapping = 2
    }

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
    }

    private FixtureMode currentMode;

    public FixtureMode CurrentMode { get => currentMode; }

    public void SetFixtureMode(FixtureMode mode)
    {
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