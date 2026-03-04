using UnityEngine;

public class PatternGenerator : MonoBehaviour
{
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

        DmxBuffer dmxBuffer = artNetReceiver.DmxBuffer;

        int patternType = Mathf.Clamp(dmxBuffer.GetChannel1Based(5) / 43, 0, 5);
        float speed = Mathf.Lerp(0.1f, 8f, dmxBuffer.GetChannel1Based(6) / 255f);
        float size = Mathf.Lerp(0.5f, 8f, dmxBuffer.GetChannel1Based(7) / 255f);

        float strobe = dmxBuffer.GetChannel1Based(8) / 255f;
        float strobeFrequency = Mathf.Lerp(1f, 20f, strobe);
        float strobeGate = (strobe < 0.05f || Mathf.Sin(Time.time * strobeFrequency) > 0f) ? 1f : 0f;

        _outputMaterial.SetInt("_PatternType", patternType);
        _outputMaterial.SetFloat("_Speed", speed);
        _outputMaterial.SetFloat("_Size", size);
        _outputMaterial.SetFloat("_StrobeGate", strobeGate);
    }
}
