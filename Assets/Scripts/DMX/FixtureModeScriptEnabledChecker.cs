using System;
using UnityEngine;

public class FixtureModeScriptEnabledChecker : MonoBehaviour
{
    void Awake()
    {
        DmxModeManager.OnModeChanged += EnableScript;
        DmxModeManager.Awoken += EnableScript;
    }

    
    void EnableScript()
    {
        EnableScript(DmxModeManager.Instance.CurrentMode);
    }

    private void EnableScript(FixtureMode mode)
    {
        if(mode == FixtureMode.Standard)
        {
            EnableSurfacePatternGeneratorScripts();
        }
        if (mode == FixtureMode.MovingHead)
        {
            EnableMovingHeadControllers();
        }
        if (mode == FixtureMode.PixelMapping)
        {
            EnablePixelControllers();
        }
    }

    private void EnablePixelControllers()
    {
        foreach (PixelMappingOutputController pixelMappingOutputController in GetComponentsInChildren<PixelMappingOutputController>())
        {
            pixelMappingOutputController.enabled = true;
        }
    }

    private void EnableMovingHeadControllers()
    {
        foreach (MovingHeadBeamController movingHeadBeamController in GetComponentsInChildren<MovingHeadBeamController>())
        {
            movingHeadBeamController.enabled = true;
        }
    }

    private void EnableSurfacePatternGeneratorScripts()
    {
        foreach (SurfacePatternGenerator surfacePatternGenerator in GetComponentsInChildren<SurfacePatternGenerator>())
        {
            surfacePatternGenerator.enabled = true;
        }
    }
}
