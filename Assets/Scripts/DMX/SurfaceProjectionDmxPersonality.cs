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

    public static Color ParseColor(DmxFixture dmxFixture, DmxFrame frame)
    {
        return new Color(
            dmxFixture.GetChannelValue(frame,RedChannel) / 255f,
            dmxFixture.GetChannelValue(frame,GreenChannel) / 255f,
            dmxFixture.GetChannelValue(frame,BlueChannel) / 255f,
            1f);
    }

    public static float ParseMasterDimmer(DmxFixture dmxFixture, DmxFrame frame)
    {
        return dmxFixture.GetChannelValue(frame,MasterDimmerChannel) / 255f;
    }
}
