using UnityEngine;
public abstract class BaseRawDmxConsumer : MonoBehaviour
{
    protected DmxFixture _fixture;
    protected virtual void Awake()
    {
        _fixture = GetComponent<DmxFixture>();
        if (_fixture == null)
            _fixture = gameObject.AddComponent<DmxFixture>();
    }

    protected virtual void OnEnable()
    {
        DmxDataService.OnFrameReceived += HandleFrameChange;
    }

    protected virtual void OnDisable()
    {
        DmxDataService.OnFrameReceived -= HandleFrameChange;
    }

    protected abstract void HandleFrameChange(DmxFrame frame);

    protected abstract bool IsActiveMode();
}
