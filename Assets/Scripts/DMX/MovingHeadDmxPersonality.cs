using UnityEngine;

public static class MovingHeadDmxPersonality
{
    public const int ChannelCount = 14;
    private const int PatternCount = 20;

    public struct Snapshot
    {
        public float MasterDimmer;
        public Color Color;
        public float PanNormalized;
        public float TiltNormalized;
        public int PatternType;
        public float PatternSpeed;
        public float PatternSize;
        public float BeamSoftness;
        public float IrisScale;
        public float RotateRadians;
        public float StrobeGate;
    }

    public static Snapshot Parse(ArtNetReceiver receiver, float timeSeconds)
    {
        float masterDimmer = receiver.GetFixtureChannelValue(1) / 255f;
        float red = receiver.GetFixtureChannelValue(2) / 255f;
        float green = receiver.GetFixtureChannelValue(3) / 255f;
        float blue = receiver.GetFixtureChannelValue(4) / 255f;

        float panNormalized = Parse16BitNormalized(
            receiver.GetFixtureChannelValue(5),
            receiver.GetFixtureChannelValue(6));

        float tiltNormalized = Parse16BitNormalized(
            receiver.GetFixtureChannelValue(7),
            receiver.GetFixtureChannelValue(8));

        float patternSelect = receiver.GetFixtureChannelValue(9);
        int patternType = Mathf.Clamp(Mathf.FloorToInt((patternSelect / 256f) * PatternCount), 0, PatternCount - 1);

        float speed = Mathf.Lerp(0.1f, 8f, receiver.GetFixtureChannelValue(10) / 255f);
        float parameter = receiver.GetFixtureChannelValue(11) / 255f;
        float size = Mathf.Lerp(0.5f, 8f, parameter);
        float beamSoftness = Mathf.Lerp(0.001f, 0.5f, parameter);

        float irisScale = Mathf.Lerp(0.05f, 1f, receiver.GetFixtureChannelValue(12) / 255f);
        float rotateRadians = (receiver.GetFixtureChannelValue(13) / 255f) * Mathf.PI * 2f;

        float strobeValue = receiver.GetFixtureChannelValue(14) / 255f;
        float strobeFrequency = Mathf.Lerp(1f, 20f, strobeValue);
        float strobeGate = (strobeValue < 0.05f || Mathf.Sin(timeSeconds * strobeFrequency) > 0f) ? 1f : 0f;

        return new Snapshot
        {
            MasterDimmer = masterDimmer,
            Color = new Color(red, green, blue),
            PanNormalized = panNormalized,
            TiltNormalized = tiltNormalized,
            PatternType = patternType,
            PatternSpeed = speed,
            PatternSize = size,
            //Commented out BeamSoftness because that is not currently working nicely. I could add a seperate DMX channel for this
            //BeamSoftness = beamSoftness,
            IrisScale = irisScale,
            RotateRadians = rotateRadians,
            StrobeGate = strobeGate
        };
    }

    private static float Parse16BitNormalized(int coarse, int fine)
    {
        int combined = (Mathf.Clamp(coarse, 0, 255) << 8) | Mathf.Clamp(fine, 0, 255);
        return combined / 65535f;
    }
}
