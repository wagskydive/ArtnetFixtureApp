using System;
using UnityEngine;

public class DmxSettingsService : MonoBehaviour
{
    public DmxSettingsSnapshot CurrentDmxSettings { get; private set; }
    public static DmxSettingsService Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Load()
    {
        int universe = SaveLoadSettings.LoadInt(SaveLoadSettings.DmxUniverseKey, 1);
        int channel = SaveLoadSettings.LoadInt(SaveLoadSettings.DmxChannelKey, 1);

        int networkMode = SaveLoadSettings.LoadInt(SaveLoadSettings.NetworkModeKey, 0);

        bool isSAcn = networkMode == 1;

        universe = ClampUniverse(universe, isSAcn);
        channel = Mathf.Clamp(channel, 1, 512);
        SAcnParameters parameters;

        if (isSAcn)
        {
            parameters = SAcnParameters.Load();
            parameters.Clamp();
        }
        else
        {
            parameters = new SAcnParameters();
        }

        CurrentDmxSettings = new DmxSettingsSnapshot(
            universe,
            channel,
            isSAcn,
            parameters
        );
        DmxSettingsBus.Publish(CurrentDmxSettings);
    }

    public void Save(DmxSettingsSnapshot snapshot)
    {
        CurrentDmxSettings = snapshot;

        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxUniverseKey, snapshot.Universe1Based);
        SaveLoadSettings.SaveInt(SaveLoadSettings.DmxChannelKey, snapshot.StartChannel);

        SaveLoadSettings.SaveInt(SaveLoadSettings.NetworkModeKey, snapshot.IsSAcnMode ? 1 : 0);


        SAcnParameters.Save(snapshot.CurrentSAcnParameters);
        SaveLoadSettings.SaveAndInvokeEvent();

        DmxSettingsBus.Publish(CurrentDmxSettings);
    }

    public static int ClampUniverse(int value, bool isSAcn)
    {
        if (isSAcn)
            return Mathf.Clamp(value, 1, 63999);

        return Mathf.Clamp(value, 1, 32768);
    }
}