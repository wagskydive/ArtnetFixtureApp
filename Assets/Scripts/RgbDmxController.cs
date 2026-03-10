using UnityEngine;

public class RgbDmxController : MonoBehaviour
{
    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] private Renderer outputRenderer;

    private Material _outputMaterial;
    private Material _activeSharedMaterial;

    private void Awake()
    {
        ResolveOutputMaterial();
    }

    private void Update()
    {
        if (artNetReceiver == null || artNetReceiver.DmxBuffer == null || !ResolveOutputMaterial())
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
    private bool ResolveOutputMaterial()
    {
        if (outputRenderer == null || outputRenderer.sharedMaterial == null)
        {
            return false;
        }

        if (_outputMaterial == null || _activeSharedMaterial != outputRenderer.sharedMaterial)
        {
            _activeSharedMaterial = outputRenderer.sharedMaterial;
            _outputMaterial = outputRenderer.material;
        }

        return _outputMaterial != null;
    }

}
