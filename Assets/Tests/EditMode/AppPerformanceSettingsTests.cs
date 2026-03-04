using NUnit.Framework;
using UnityEngine;

public class AppPerformanceSettingsTests
{
    [Test]
    public void Apply_SetsTargetFrameRateAndDisablesVSync()
    {
        var go = new GameObject("perf-settings");
        var settings = go.AddComponent<AppPerformanceSettings>();

        typeof(AppPerformanceSettings)
            .GetField("targetFrameRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(settings, 30);
        typeof(AppPerformanceSettings)
            .GetField("disableVSync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(settings, true);

        settings.Apply();

        Assert.That(Application.targetFrameRate, Is.EqualTo(30));
        Assert.That(QualitySettings.vSyncCount, Is.EqualTo(0));

        Object.DestroyImmediate(go);
    }
}
