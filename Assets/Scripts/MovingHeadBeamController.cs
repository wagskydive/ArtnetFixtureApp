using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;

public class MovingHeadBeamController : BaseDmxMaterialConsumer
{
    private const string CustomGoboCapabilityId = "capability.custom.gobos";
    private Texture _fallbackGoboTexture;
    private Texture _activeGoboTexture;
    private readonly Texture2D[] _customGoboSlotTextures = new Texture2D[CustomGoboStorage.MaxSlots];
    private readonly long[] _customGoboSlotWriteTicks = new long[CustomGoboStorage.MaxSlots];
    private readonly List<Texture2D> _availableCustomGobos = new List<Texture2D>(CustomGoboStorage.MaxSlots);
    private float _nextGoboReloadTime;
    private bool _hasLoadedCustomGobos;

    protected override Renderer GetRenderer() => GetComponent<Renderer>();

    protected override bool IsActiveMode()
    {
        return DmxModeManager.Instance != null &&
               DmxModeManager.Instance.CurrentMode == DmxModeManager.FixtureMode.MovingHead;
    }

    private void ApplyCustomGoboTexture(int patternType, float speed)
    {
        if (!ResolveMaterial() || patternType != 1 || !IsCustomGoboUnlocked())
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
        if (_material != null && _fallbackGoboTexture != null)
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
        if (_material == null || texture == null || ReferenceEquals(_activeGoboTexture, texture))
        {
            return;
        }

        _activeGoboTexture = texture;
        _material.SetTexture("_GoboTex", texture);
    }



    protected override void OnDmxFrame(DmxFrame frame)
    {
        if (!ResolveMaterial() || _fixture == null)
            return;

        var snapshot = MovingHeadDmxPersonality.Parse(_fixture, frame, Time.time);

        _material.SetColor("_Color", snapshot.Color);
        _material.SetFloat("_Intensity", snapshot.MasterDimmer);

        _material.SetInt("_PatternType", snapshot.PatternType);
        _material.SetFloat("_Speed", snapshot.PatternSpeed);
        _material.SetFloat("_Size", snapshot.PatternSize);
        _material.SetFloat("_StrobeGate", snapshot.StrobeGate);

        _material.SetFloat("_BeamOffsetX", Mathf.Lerp(-1f, 1f, snapshot.PanNormalized));
        _material.SetFloat("_BeamOffsetY", Mathf.Lerp(-1f, 1f, snapshot.TiltNormalized));
        _material.SetFloat("_BeamSoftness", snapshot.BeamSoftness);
        _material.SetFloat("_BeamRadius", snapshot.IrisScale);
        _material.SetFloat("_BeamRotation", snapshot.RotateRadians);
        ApplyCustomGoboTexture(snapshot.PatternType, snapshot.PatternSpeed);
    }
}
