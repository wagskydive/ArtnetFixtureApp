using UnityEngine;

public class RgbDmxController : MonoBehaviour
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

    private void Update()
    {
        if (artNetReceiver == null || artNetReceiver.DmxBuffer == null || _outputMaterial == null)
        {
            return;
        }

        DmxBuffer dmxBuffer = artNetReceiver.DmxBuffer;

        float dimmer = dmxBuffer.GetChannel1Based(1) / 255f;
        float red = dmxBuffer.GetChannel1Based(2) / 255f;
        float green = dmxBuffer.GetChannel1Based(3) / 255f;
        float blue = dmxBuffer.GetChannel1Based(4) / 255f;

        _outputMaterial.SetColor("_Color", new Color(red, green, blue, 1f));
        _outputMaterial.SetFloat("_Intensity", dimmer);
    }
}
