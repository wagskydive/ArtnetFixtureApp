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
        DmxDataService.OnDmxFrame += HandleFrame;
    }

    protected virtual void OnDisable()
    {
        DmxDataService.OnDmxFrame -= HandleFrame;
    }

    protected virtual void HandleFrame(DmxFrame frame)
    {
        if (!IsActiveMode())
            return;


        OnDmxFrame(frame);
    }

    protected abstract void OnDmxFrame(DmxFrame frame);

    protected abstract bool IsActiveMode();
}
