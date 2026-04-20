using NUnit.Framework;
using UnityEngine;

public class DmxFixtureTests
{
    [Test]
    public void GetChannelValue_UsesStartChannelOverrideAddedAfterAwake()
    {
        var gameObject = new GameObject("FixtureUnderTest");
        var fixture = gameObject.AddComponent<DmxFixture>();

        // Add override after DmxFixture.Awake has already run.
        var startChannelOverride = gameObject.AddComponent<StartChannelOverride>();
        startChannelOverride.SetFixtureIndex(1);
        startChannelOverride.SetFixtureDmxChannelAmount(16);

        var snapshot = new DmxSettingsSnapshot(1, 1, false, SAcnParameters.Default());
        fixture.ApplyDmxSettings(snapshot);

        var buffer = new byte[512];
        buffer[16] = 200; // Absolute channel 17, fixture index 1 with 16 channels per fixture.
        var frame = new DmxFrame(buffer);

        int channelValue = fixture.GetChannelValue(frame, 1);

        Assert.AreEqual(200, channelValue);

        Object.DestroyImmediate(gameObject);
    }
}
