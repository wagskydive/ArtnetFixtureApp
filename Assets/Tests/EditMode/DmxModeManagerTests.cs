using NUnit.Framework;
using UnityEngine;

public class DmxModeManagerTests
{
    [Test]
    public void SetFixtureMode_SameMode_DoesNotRaiseModeChanged()
    {
        var go = new GameObject("mode-manager");
        var manager = go.AddComponent<DmxModeManager>();

        int callCount = 0;
        System.Action<DmxModeManager.FixtureMode> handler = _ => callCount++;
        DmxModeManager.OnModeChanged += handler;

        manager.SetFixtureMode(DmxModeManager.FixtureMode.Standard);
        manager.SetFixtureMode(DmxModeManager.FixtureMode.Standard);

        DmxModeManager.OnModeChanged -= handler;
        Assert.That(callCount, Is.EqualTo(0));

        Object.DestroyImmediate(go);
    }
}
