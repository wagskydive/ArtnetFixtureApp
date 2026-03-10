using UnityEngine;

public class PixelMappingOutputController : MonoBehaviour
{
    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] private Renderer outputRenderer;
    [SerializeField] private UI_FixtureModeSelector fixtureModeSelector;
    [SerializeField] private int fallbackRows = 8;
    [SerializeField] private int fallbackColumns = 8;

    private Material _outputMaterial;
    private Material _activeSharedMaterial;
    private Texture2D _pixelDataTexture;
    private Color32[] _pixelBuffer;
    private int _lastRows;
    private int _lastColumns;

    bool isInMode;

    private void Awake()
    {
        if (outputRenderer != null)
        {
            _outputMaterial = outputRenderer.material;
        }

        EnsureTexture();
    }
    private void Start()
    {
        DmxModeManager.OnModeChanged += HandleModeChange;
        isInMode = DmxModeManager.Instance.CurrentMode == DmxModeManager.FixtureMode.PixelMapping;
    }

    private void OnDestroy()
    {
        if (_pixelDataTexture != null)
        {
            Destroy(_pixelDataTexture);
            _pixelDataTexture = null;
        }
    }

    void HandleModeChange(DmxModeManager.FixtureMode mode)
    {
        isInMode = mode == DmxModeManager.FixtureMode.PixelMapping;

    }


    private void Update()
    {
        if (artNetReceiver == null || artNetReceiver.DmxBuffer == null || !ResolveOutputMaterial() || !isInMode)
        {
            return;
        }

        EnsureTexture();

        float master = PixelMappingDmxPersonality.ParseMasterDimmer(artNetReceiver);
        float strobeGate = PixelMappingDmxPersonality.ParseStrobeGate(artNetReceiver, Time.time);

        PixelMappingDmxPersonality.ParsePixelColors(artNetReceiver, _lastRows, _lastColumns, _pixelBuffer);
        _pixelDataTexture.SetPixels32(_pixelBuffer);
        _pixelDataTexture.Apply(false, false);

        _outputMaterial.SetFloat("_Rows", _lastRows);
        _outputMaterial.SetFloat("_Columns", _lastColumns);
        _outputMaterial.SetFloat("_Intensity", master);
        _outputMaterial.SetFloat("_StrobeGate", strobeGate);
        _outputMaterial.SetFloat("_UsePixelDataTex", 1f);
        _outputMaterial.SetTexture("_PixelDataTex", _pixelDataTexture);
    }

    private void EnsureTexture()
    {
        int rows = fixtureModeSelector != null ? fixtureModeSelector.CurrentPixelRows : Mathf.Clamp(fallbackRows, 1, 32);
        int columns = fixtureModeSelector != null ? fixtureModeSelector.CurrentPixelColumns : Mathf.Clamp(fallbackColumns, 1, 32);

        if (_pixelDataTexture != null && rows == _lastRows && columns == _lastColumns)
        {
            return;
        }

        _lastRows = rows;
        _lastColumns = columns;

        if (_pixelDataTexture != null)
        {
            Destroy(_pixelDataTexture);
        }

        _pixelDataTexture = new Texture2D(columns, rows, TextureFormat.RGBA32, false, true)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            name = "PixelMappingDmxData"
        };

        _pixelBuffer = new Color32[rows * columns];
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
