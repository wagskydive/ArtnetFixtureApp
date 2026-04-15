using UnityEngine;
public abstract class BaseDmxMaterialConsumer : BaseRawDmxConsumer
{

    protected Material _material;
    private Material _lastShared;



    protected abstract Renderer GetRenderer();

    protected bool ResolveMaterial()
    {
        var renderer = GetRenderer();
        if (renderer == null || renderer.sharedMaterial == null)
            return false;

        if (_material == null || _lastShared != renderer.sharedMaterial)
        {
            _lastShared = renderer.sharedMaterial;
            _material = renderer.material;
            OnMaterialChanged();
        }

        return _material != null;
    }

    protected override void HandleFrame(DmxFrame frame)
    {
        if (!IsActiveMode())
            return;

        if (!ResolveMaterial())
            return;

        OnDmxFrame(frame);
    }

    protected virtual void OnMaterialChanged() { }
}