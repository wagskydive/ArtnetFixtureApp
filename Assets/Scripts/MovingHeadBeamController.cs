using UnityEngine;
using System.Collections.Generic;

public class MovingHeadBeamController : MonoBehaviour
{
    private const string CustomGoboCapabilityId = "capability.custom.gobos";

    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] private Renderer outputRenderer;

    private Material _outputMaterial;
    private Material _activeSharedMaterial;
    private Texture _fallbackGoboTexture;
    private readonly List<Texture2D> _customGoboTextures = new List<Texture2D>(CustomGoboStorage.MaxSlots);
    private float _nextGoboReloadTime;
    private bool _hasLoadedCustomGobos;

    bool isInMode;

    private void Awake()
    {
        ResolveOutputMaterial();

    }
    private void Start()
    {
        DmxModeManager.OnModeChanged += HandleModeChange;
        isInMode = DmxModeManager.Instance.CurrentMode == DmxModeManager.FixtureMode.MovingHead;
    
    }

    void HandleModeChange(DmxModeManager.FixtureMode mode)
    {
        isInMode = mode == DmxModeManager.FixtureMode.MovingHead;

    }


    private void Update()
    {
        if (artNetReceiver == null || artNetReceiver.DmxBuffer == null || !ResolveOutputMaterial() || !isInMode)
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
        ApplyCustomGoboTexture(snapshot.PatternType, snapshot.PatternSpeed);
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
            _fallbackGoboTexture = _outputMaterial != null ? _outputMaterial.GetTexture("_GoboTex") : null;
            _hasLoadedCustomGobos = false;
        }

        return _outputMaterial != null;
    }

    private void ApplyCustomGoboTexture(int patternType, float speed)
    {
        if (_outputMaterial == null || patternType != 1 || !IsCustomGoboUnlocked())
        {
            SetFallbackGoboTexture();
            return;
        }

        if (!_hasLoadedCustomGobos || Time.time >= _nextGoboReloadTime)
        {
            ReloadCustomGobos();
            _nextGoboReloadTime = Time.time + 2f;
        }

        if (_customGoboTextures.Count == 0)
        {
            SetFallbackGoboTexture();
            return;
        }

        float normalizedSelector = Mathf.Clamp01((speed - 0.1f) / (8f - 0.1f));
        int index = Mathf.Clamp(Mathf.FloorToInt(normalizedSelector * _customGoboTextures.Count), 0, _customGoboTextures.Count - 1);
        _outputMaterial.SetTexture("_GoboTex", _customGoboTextures[index]);
    }

    private bool IsCustomGoboUnlocked()
    {
        if (CapabilityService.Instance == null)
        {
            return false;
        }

        return CapabilityService.Instance.ResolveBoolean(CustomGoboCapabilityId, false);
    }

    private void SetFallbackGoboTexture()
    {
        if (_outputMaterial != null && _fallbackGoboTexture != null)
        {
            _outputMaterial.SetTexture("_GoboTex", _fallbackGoboTexture);
        }
    }

    private void ReloadCustomGobos()
    {
        ReleaseCustomGobos();

        for (int slot = 1; slot <= CustomGoboStorage.MaxSlots; slot++)
        {
            Texture2D texture = CustomGoboStorage.LoadSlotTexture(slot);
            if (texture != null)
            {
                _customGoboTextures.Add(texture);
            }
        }

        _hasLoadedCustomGobos = true;
    }

    private void ReleaseCustomGobos()
    {
        for (int i = 0; i < _customGoboTextures.Count; i++)
        {
            if (_customGoboTextures[i] != null)
            {
                Destroy(_customGoboTextures[i]);
            }
        }

        _customGoboTextures.Clear();
    }

    private void OnDestroy()
    {
        ReleaseCustomGobos();
        DmxModeManager.OnModeChanged -= HandleModeChange;
    }
}
