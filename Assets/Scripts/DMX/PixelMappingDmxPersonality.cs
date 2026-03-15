using UnityEngine;

public static class PixelMappingDmxPersonality
{
    public const int MasterDimmerChannel = 1;
    public const int StrobeChannel = 2;
    public const int CornerPinStartChannel = 3;
    public const int CornerPinChannelCount = 8;
    public const int PixelDataStartChannel = 11;

    public static float ParseMasterDimmer(ArtNetReceiver receiver)
    {
        return receiver.GetFixtureChannelValue(MasterDimmerChannel) / 255f;
    }

    public static float ParseStrobeGate(ArtNetReceiver receiver, float timeSeconds)
    {
        float strobe = receiver.GetFixtureChannelValue(StrobeChannel) / 255f;
        float strobeFrequency = Mathf.Lerp(1f, 50f, strobe);
        return (strobe < 0.05f || Mathf.Sin(timeSeconds * strobeFrequency) > 0f) ? 1f : 0f;
    }

    public static void ParsePixelColors(ArtNetReceiver receiver, int rows, int columns, Color32[] destination)
    {
        int pixelCount = Mathf.Max(0, rows) * Mathf.Max(0, columns);
        int safeCount = Mathf.Min(pixelCount, destination.Length);

        for (int pixelIndex = 0; pixelIndex < safeCount; pixelIndex++)
        {
            int baseChannel = PixelDataStartChannel + (pixelIndex * 3);

            byte red = (byte)receiver.GetFixtureChannelValue(baseChannel);
            byte green = (byte)receiver.GetFixtureChannelValue(baseChannel + 1);
            byte blue = (byte)receiver.GetFixtureChannelValue(baseChannel + 2);
            destination[pixelIndex] = new Color32(red, green, blue, 255);
        }

        for (int pixelIndex = safeCount; pixelIndex < destination.Length; pixelIndex++)
        {
            destination[pixelIndex] = new Color32(0, 0, 0, 255);
        }
    }
}
