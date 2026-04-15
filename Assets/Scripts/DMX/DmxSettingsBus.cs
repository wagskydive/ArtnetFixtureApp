using System;

public static class DmxSettingsBus
{
    public static event Action<DmxSettingsSnapshot> OnChanged;

    public static void Publish(DmxSettingsSnapshot snapshot)
    {
        OnChanged?.Invoke(snapshot);
    }
}