using UnityEngine;

public class ProjectorLightOutput : MonoBehaviour
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

        float dimmer = dmxBuffer.GetChannel1Based(1) / 255f;
        float r = dmxBuffer.GetChannel1Based(2) / 255f;
        float g = dmxBuffer.GetChannel1Based(3) / 255f;
        float b = dmxBuffer.GetChannel1Based(4) / 255f;

        _outputMaterial.SetColor("_Color", new Color(r, g, b, 1f));
        _outputMaterial.SetFloat("_Intensity", dimmer);
    }
}
