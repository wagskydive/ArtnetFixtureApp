using System;
using UnityEngine;

public class DmxSettingsService : MonoBehaviour
{
    public DmxSettingsSnapshot CurrentDmxSettings { get; private set; }
    public static DmxSettingsService Instance;

    public static event Action<DmxSettingsSnapshot> OnLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        DmxSettingsBus.OnChanged += HandleDmxSettingsChanged;

    }
    void OnDsable()
    {
        DmxSettingsBus.OnChanged -= HandleDmxSettingsChanged;

    }

    private void HandleDmxSettingsChanged(DmxSettingsSnapshot snapshot)
    {
        CurrentDmxSettings = snapshot;
    }

    public void Load()
    {
        int universe = SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey, 1);
        int channel = SaveLoadSettings.LoadInt(SaveLoadSettings.DmxChannelKey, 1);

        int networkMode = SaveLoadSettings.LoadInt(SaveLoadSettings.NetworkModeKey, 0);

        bool isSAcn = networkMode == 1;

        universe = ClampUniverse(universe, isSAcn);
        channel = Mathf.Clamp(channel, 1, 512);


        CurrentDmxSettings = new DmxSettingsSnapshot(
            universe,
            channel,
            isSAcn,
            SAcnParameters.Load()
        );
        OnLoaded?.Invoke(CurrentDmxSettings);
    }



    public static int ClampUniverse(int value, bool isSAcn)
    {
        if (isSAcn)
            return Mathf.Clamp(value, 1, 63999);

        return Mathf.Clamp(value, 1, 32768);
    }
}