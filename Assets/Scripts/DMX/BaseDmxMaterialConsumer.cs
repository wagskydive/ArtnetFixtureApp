using UnityEngine;
public abstract class BaseDmxMaterialConsumer : BaseRawDmxConsumer
{

    protected Material _material;
    private Material _lastShared;

    void Start()
    {
        ResolveMaterial();
    }

    protected Renderer GetRenderer()
    {
        return GetComponent<Renderer>();
    }

    void Update()
    {
        if (!IsActiveMode() || _fixture == null)
            return;

        OnDmxFrame(DmxDataService.LatestFrame);
    }

    protected bool ResolveMaterial()
    {
        var renderer = GetRenderer();
        if (renderer == null || renderer.sharedMaterial == null)
        {
            return false;
        }

        if (_material == null || _lastShared != renderer.sharedMaterial)
        {
            _lastShared = renderer.sharedMaterial;
            _material = renderer.material;
            OnMaterialChanged();
        }

        return _material != null;
    }

    protected abstract void OnDmxFrame(DmxFrame frame);

    protected override void HandleFrameChange(DmxFrame frame)
    {
        ResolveMaterial();
    }

    protected virtual void OnMaterialChanged() { }
}