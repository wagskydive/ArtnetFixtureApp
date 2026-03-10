using UnityEngine;

public class MovingHeadBeamController : MonoBehaviour
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

        float pan = artNetReceiver.GetFixtureChannelValue(5) / 255f;
        float tilt = artNetReceiver.GetFixtureChannelValue(7) / 255f;
        float parameter = artNetReceiver.GetFixtureChannelValue(11) / 255f;
        float irisScale = artNetReceiver.GetFixtureChannelValue(12) / 255f;
        float rotation = artNetReceiver.GetFixtureChannelValue(13) / 255f;

        _outputMaterial.SetFloat("_BeamOffsetX", Mathf.Lerp(-1f, 1f, pan));
        _outputMaterial.SetFloat("_BeamOffsetY", Mathf.Lerp(-1f, 1f, tilt));
        _outputMaterial.SetFloat("_BeamSoftness", Mathf.Lerp(0.001f, 0.5f, parameter));
        _outputMaterial.SetFloat("_BeamRadius", Mathf.Lerp(0.05f, 1f, irisScale));
        _outputMaterial.SetFloat("_BeamRotation", rotation * Mathf.PI * 2f);
    }
}
