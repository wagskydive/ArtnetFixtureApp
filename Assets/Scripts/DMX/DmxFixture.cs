using UnityEngine;

public class DmxFixture : MonoBehaviour, IDmxSettingsConsumer
{
    [SerializeField] private int startChannel = 1;

    private StartChannelOverride _override;

    private void Awake()
    {
        _override = GetComponent<StartChannelOverride>();
    }

    void OnEnable()
    {
        DmxSettingsBus.OnChanged += ApplyDmxSettings;
    }

    void OnDisable()
    {
        DmxSettingsBus.OnChanged -= ApplyDmxSettings;
    }

    public int GetChannelValue(DmxFrame frame, int relativeChannel)
    {
        if (frame.Buffer == null)
            return 0;

        EnsureOverrideReference();
        int offset = _override != null ? _override.GetChannelOffset() : 0;

        int absoluteChannel = startChannel + offset + relativeChannel - 1;

        if (absoluteChannel < 1 || absoluteChannel > 512)
            return 0;

        return frame.Buffer[absoluteChannel - 1];
    }

    public void ApplyDmxSettings(DmxSettingsSnapshot snapshot)
    {
        startChannel = snapshot.StartChannel;
    }

    private void EnsureOverrideReference()
    {
        if (_override == null)
        {
            _override = GetComponent<StartChannelOverride>();
        }
    }
}
