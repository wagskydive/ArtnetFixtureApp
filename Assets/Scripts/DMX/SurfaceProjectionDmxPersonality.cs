using UnityEngine;

public static class SurfaceProjectionDmxPersonality
{
    public const int MasterDimmerChannel = 1;
    public const int RedChannel = 2;
    public const int GreenChannel = 3;
    public const int BlueChannel = 4;
    public const int PatternTypeChannel = 5;
    public const int PatternSpeedChannel = 6;
    public const int PatternSizeChannel = 7;
    public const int StrobeChannel = 8;
    public const int CornerPinStartChannel = 9;
    public const int CornerPinChannelCount = 8;

    public static Color ParseColor(ArtNetReceiver receiver)
    {
        return new Color(
            receiver.GetFixtureChannelValue(RedChannel) / 255f,
            receiver.GetFixtureChannelValue(GreenChannel) / 255f,
            receiver.GetFixtureChannelValue(BlueChannel) / 255f,
            1f);
    }

    public static float ParseMasterDimmer(ArtNetReceiver receiver)
    {
        return receiver.GetFixtureChannelValue(MasterDimmerChannel) / 255f;
    }
}
