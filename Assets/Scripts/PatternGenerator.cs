using UnityEngine;

public class PatternGenerator : MonoBehaviour
{
    private const int PatternCount = 10;

    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] private Renderer outputRenderer;

    private Material _outputMaterial;

    private void Awake()
    {
        if (outputRenderer != null)
        {
            _outputMaterial = outputRenderer.material;
        }
    }

    void Update()
    {
        if (artNetReceiver == null || artNetReceiver.DmxBuffer == null || _outputMaterial == null)
        {
            return;
        }

        int dmxPatternValue = artNetReceiver.GetFixtureChannelValue(5);
        int patternType = Mathf.Clamp(Mathf.FloorToInt((dmxPatternValue / 256f) * PatternCount), 0, PatternCount - 1);
        float speed = Mathf.Lerp(0.1f, 8f, artNetReceiver.GetFixtureChannelValue(6) / 255f);
        float size = Mathf.Lerp(0.5f, 8f, artNetReceiver.GetFixtureChannelValue(7) / 255f);

        float strobe = artNetReceiver.GetFixtureChannelValue(8) / 255f;
        float strobeFrequency = Mathf.Lerp(1f, 20f, strobe);
        float strobeGate = (strobe < 0.05f || Mathf.Sin(Time.time * strobeFrequency) > 0f) ? 1f : 0f;

        _outputMaterial.SetInt("_PatternType", patternType);
        _outputMaterial.SetFloat("_Speed", speed);
        _outputMaterial.SetFloat("_Size", size);
        _outputMaterial.SetFloat("_StrobeGate", strobeGate);
    }
}
