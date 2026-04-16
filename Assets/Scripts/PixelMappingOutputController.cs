using UnityEngine;

public class PixelMappingOutputController : BaseDmxMaterialConsumer
{
    [SerializeField] private UI_FixtureModeSelector fixtureModeSelector;
    [SerializeField] private int fallbackRows = 8;
    [SerializeField] private int fallbackColumns = 8;

    private Texture2D _pixelDataTexture;
    private Color32[] _pixelBuffer;
    private int _lastRows;
    private int _lastColumns;

    protected override Renderer GetRenderer() => GetComponent<Renderer>();

    protected override bool IsActiveMode()
    {
        return DmxModeManager.Instance != null &&
               DmxModeManager.Instance.CurrentMode == FixtureMode.PixelMapping;
    }



    private void OnDestroy()
    {
        if (_pixelDataTexture != null)
        {
            Destroy(_pixelDataTexture);
            _pixelDataTexture = null;
        }
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

    protected override void OnDmxFrame(DmxFrame frame)
    {
        if (!ResolveMaterial() || _fixture == null)
            return;

        EnsureTexture();

        float master = PixelMappingDmxPersonality.ParseMasterDimmer(_fixture, frame);
        float strobeGate = PixelMappingDmxPersonality.ParseStrobeGate(_fixture, frame, Time.time);

        PixelMappingDmxPersonality.ParsePixelColors(_fixture, frame, _lastRows, _lastColumns, _pixelBuffer);
        _pixelDataTexture.SetPixels32(_pixelBuffer);
        _pixelDataTexture.Apply(false, false);

        _material.SetFloat("_Rows", _lastRows);
        _material.SetFloat("_Columns", _lastColumns);
        _material.SetFloat("_Intensity", master);
        _material.SetFloat("_StrobeGate", strobeGate);
        _material.SetFloat("_UsePixelDataTex", 1f);
        _material.SetTexture("_PixelDataTex", _pixelDataTexture);
    }
}
