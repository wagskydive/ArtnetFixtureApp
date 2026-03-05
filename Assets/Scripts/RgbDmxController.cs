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

        float dimmer = artNetReceiver.GetFixtureChannelValue(1) / 255f;
        float red = artNetReceiver.GetFixtureChannelValue(2) / 255f;
        float green = artNetReceiver.GetFixtureChannelValue(3) / 255f;
        float blue = artNetReceiver.GetFixtureChannelValue(4) / 255f;

        _outputMaterial.SetColor("_Color", new Color(red, green, blue, 1f));
        _outputMaterial.SetFloat("_Intensity", dimmer);
    }
}
