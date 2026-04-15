using UnityEngine;

public class SurfacePatternGenerator : BaseDmxMaterialConsumer
{
    private const int PatternCount = 20;

    protected override Renderer GetRenderer() => GetComponent<Renderer>();

    protected override bool IsActiveMode()
    {
        return DmxModeManager.Instance != null &&
               DmxModeManager.Instance.CurrentMode == DmxModeManager.FixtureMode.Standard;
    }

    protected override void OnDmxFrame(DmxFrame frame)
    {
        if (!ResolveMaterial() || _fixture == null)
            return;

        int dmxPatternValue = _fixture.GetChannelValue(frame,5);
        int patternType = Mathf.Clamp(Mathf.FloorToInt((dmxPatternValue / 256f) * PatternCount), 0, PatternCount - 1);
        float speed = Mathf.Lerp(0.1f, 8f, _fixture.GetChannelValue(frame,6) / 255f);
        float size = Mathf.Lerp(0.5f, 8f, _fixture.GetChannelValue(frame,7) / 255f);

        float strobe = _fixture.GetChannelValue(frame,8) / 255f;
        float strobeFrequency = Mathf.Lerp(1f, 50f, strobe);
        float strobeGate = (strobe < 0.05f || Mathf.Sin(Time.time * strobeFrequency) > 0f) ? 1f : 0f;

        _material.SetInt("_PatternType", patternType);
        _material.SetFloat("_Speed", speed);
        _material.SetFloat("_Size", size);
        _material.SetFloat("_StrobeGate", strobeGate);
    }

}
