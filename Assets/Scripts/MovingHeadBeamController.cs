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

        var snapshot = MovingHeadDmxPersonality.Parse(artNetReceiver, Time.time);

        _outputMaterial.SetColor("_BaseColor", snapshot.Color);
        _outputMaterial.SetFloat("_Intensity", snapshot.MasterDimmer);
        _outputMaterial.SetInt("_PatternType", snapshot.PatternType);
        _outputMaterial.SetFloat("_Speed", snapshot.PatternSpeed);
        _outputMaterial.SetFloat("_Size", snapshot.PatternSize);
        _outputMaterial.SetFloat("_StrobeGate", snapshot.StrobeGate);

        _outputMaterial.SetFloat("_BeamOffsetX", Mathf.Lerp(-1f, 1f, snapshot.PanNormalized));
        _outputMaterial.SetFloat("_BeamOffsetY", Mathf.Lerp(-1f, 1f, snapshot.TiltNormalized));
        _outputMaterial.SetFloat("_BeamSoftness", snapshot.BeamSoftness);
        _outputMaterial.SetFloat("_BeamRadius", snapshot.IrisScale);
        _outputMaterial.SetFloat("_BeamRotation", snapshot.RotateRadians);
    }
}
