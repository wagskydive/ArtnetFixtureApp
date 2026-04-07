using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class MovingHeadBeamController : MonoBehaviour
{
    private const string CustomGoboCapabilityId = "capability.custom.gobos";

        [SerializeField] private Renderer outputRenderer;

    private Material _outputMaterial;
    private Material _activeSharedMaterial;
    private Texture _fallbackGoboTexture;
    private Texture _activeGoboTexture;
    private readonly Texture2D[] _customGoboSlotTextures = new Texture2D[CustomGoboStorage.MaxSlots];
    private readonly long[] _customGoboSlotWriteTicks = new long[CustomGoboStorage.MaxSlots];
    private readonly List<Texture2D> _availableCustomGobos = new List<Texture2D>(CustomGoboStorage.MaxSlots);
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
        INetworkReceiver receiver = NetworkingModeManager.Instance?.NetworkReceiver;
        if (receiver == null || receiver.DmxBuffer == null || !ResolveOutputMaterial() || !isInMode)
        {
            return;
        }

        var snapshot = MovingHeadDmxPersonality.Parse(receiver, Time.time);

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
            _activeGoboTexture = null;
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

        if (_availableCustomGobos.Count == 0)
        {
            SetFallbackGoboTexture();
            return;
        }

        float normalizedSelector = Mathf.Clamp01((speed - 0.1f) / (8f - 0.1f));
        int index = Mathf.Clamp(Mathf.FloorToInt(normalizedSelector * _availableCustomGobos.Count), 0, _availableCustomGobos.Count - 1);
        SetGoboTexture(_availableCustomGobos[index]);
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
            SetGoboTexture(_fallbackGoboTexture);
        }
    }

    private void ReloadCustomGobos()
    {
        bool listChanged = false;

        for (int slot = 1; slot <= CustomGoboStorage.MaxSlots; slot++)
        {
            int index = slot - 1;
            string path = CustomGoboStorage.GetSlotPath(slot);
            if (!File.Exists(path))
            {
                if (_customGoboSlotTextures[index] != null)
                {
                    Destroy(_customGoboSlotTextures[index]);
                    _customGoboSlotTextures[index] = null;
                    _customGoboSlotWriteTicks[index] = 0;
                    listChanged = true;
                }

                continue;
            }

            long writeTicks = File.GetLastWriteTimeUtc(path).Ticks;
            if (_customGoboSlotTextures[index] != null && _customGoboSlotWriteTicks[index] == writeTicks)
            {
                continue;
            }

            Texture2D texture = CustomGoboStorage.LoadSlotTexture(slot);
            if (texture == null)
            {
                if (_customGoboSlotTextures[index] != null)
                {
                    Destroy(_customGoboSlotTextures[index]);
                    _customGoboSlotTextures[index] = null;
                    _customGoboSlotWriteTicks[index] = 0;
                    listChanged = true;
                }

                continue;
            }

            if (_customGoboSlotTextures[index] != null)
            {
                Destroy(_customGoboSlotTextures[index]);
            }

            _customGoboSlotTextures[index] = texture;
            _customGoboSlotWriteTicks[index] = writeTicks;
            listChanged = true;
        }

        if (listChanged || !_hasLoadedCustomGobos)
        {
            _availableCustomGobos.Clear();
            for (int i = 0; i < _customGoboSlotTextures.Length; i++)
            {
                if (_customGoboSlotTextures[i] != null)
                {
                    _availableCustomGobos.Add(_customGoboSlotTextures[i]);
                }
            }
        }

        _hasLoadedCustomGobos = true;
    }

    private void ReleaseCustomGobos()
    {
        for (int i = 0; i < _customGoboSlotTextures.Length; i++)
        {
            if (_customGoboSlotTextures[i] != null)
            {
                Destroy(_customGoboSlotTextures[i]);
                _customGoboSlotTextures[i] = null;
                _customGoboSlotWriteTicks[i] = 0;
            }
        }

        _availableCustomGobos.Clear();
    }

    private void SetGoboTexture(Texture texture)
    {
        if (_outputMaterial == null || texture == null || ReferenceEquals(_activeGoboTexture, texture))
        {
            return;
        }

        _activeGoboTexture = texture;
        _outputMaterial.SetTexture("_GoboTex", texture);
    }

    private void OnDestroy()
    {
        ReleaseCustomGobos();
        DmxModeManager.OnModeChanged -= HandleModeChange;
    }
}
