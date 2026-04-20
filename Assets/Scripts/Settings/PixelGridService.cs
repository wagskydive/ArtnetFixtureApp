using System;
using UnityEngine;

public readonly struct PixelGridSnapshot
{
    public const int MinPixelWallSize = 8;
    public const int MaxPixelWallSize = 32;
    public const int PixelWallStepSize = 8;

    public readonly int Rows;
    public readonly int Columns;

    public PixelGridSnapshot(int rows, int columns)
    {
        Rows = ClampPixelDimension(rows);
        Columns = ClampPixelDimension(columns);
    }

    public static int ClampPixelDimension(int value)
    {
        int clamped = Mathf.Clamp(value, MinPixelWallSize, MaxPixelWallSize);
        int remainder = clamped % PixelWallStepSize;
        return remainder == 0 ? clamped : clamped - remainder;
    }
}

public sealed class PixelGridService
{
    private static PixelGridService _instance;
    public static PixelGridService Instance => _instance ?? (_instance = new PixelGridService());

    public PixelGridSnapshot CurrentPixelGrid { get; private set; }

    public event Action<PixelGridSnapshot> OnLoaded;
    public event Action<PixelGridSnapshot> OnChanged;

    private PixelGridService()
    {
        SaveLoadSettings.OnPixelGridSettingsSaved += HandlePixelGridSettingsSaved;
        Load();
    }

    public void Load()
    {
        CurrentPixelGrid = new PixelGridSnapshot(
            SaveLoadSettings.LoadInt(SaveLoadSettings.PixelRowsKey, PixelGridSnapshot.MinPixelWallSize),
            SaveLoadSettings.LoadInt(SaveLoadSettings.PixelColumnsKey, PixelGridSnapshot.MinPixelWallSize)
        );

        OnLoaded?.Invoke(CurrentPixelGrid);
        OnChanged?.Invoke(CurrentPixelGrid);
    }

    public void Save(PixelGridSnapshot snapshot)
    {
        SaveLoadSettings.SavePixelGridSettings(new PixelGridSettings(snapshot.Rows, snapshot.Columns));
    }

    private void HandlePixelGridSettingsSaved(PixelGridSettings settings)
    {
        CurrentPixelGrid = new PixelGridSnapshot(settings.PixelRows, settings.PixelColumns);
        OnChanged?.Invoke(CurrentPixelGrid);
    }
}
