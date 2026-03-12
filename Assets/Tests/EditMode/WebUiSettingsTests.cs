using System.Reflection;
using NUnit.Framework;
using UnityEngine;


public class WebUiSettingsTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey("webui.device.name");
        PlayerPrefs.DeleteKey("dmx.fixture.mode");
        PlayerPrefs.DeleteKey("dmx.universe");
        PlayerPrefs.DeleteKey("dmx.channel");
        PlayerPrefs.DeleteKey("dmx.fixture.count");
        PlayerPrefs.DeleteKey("dmx.pixel.rows");
        PlayerPrefs.DeleteKey("dmx.pixel.columns");
    }

    [Test]
    public void SaveAndLoad_PersistsSanitizedWebUiSettingsInPlayerPrefs()
    {
        var dirty = new WebUiSettingsData
        {
            deviceName = "  Test Fixture  ",
            fixtureMode = "pixel",
            dmxUniverse = 999,
            startChannel = -42,
            fixtureAmount = 50,
            gridX = 31,
            gridY = 7
        };

        WebUiSettingsStore.Save(dirty);
        WebUiSettingsData loaded = WebUiSettingsStore.Load();

        Assert.That(loaded.deviceName, Is.EqualTo("Test Fixture"));
        Assert.That(loaded.fixtureMode, Is.EqualTo("pixel"));
        Assert.That(loaded.dmxUniverse, Is.EqualTo(16));
        Assert.That(loaded.startChannel, Is.EqualTo(1));
        Assert.That(loaded.fixtureAmount, Is.EqualTo(16));
        Assert.That(loaded.gridX, Is.EqualTo(24));
        Assert.That(loaded.gridY, Is.EqualTo(8));
    }

    [Test]
    public void ApplySettings_NonSurfaceModeForcesSingleFixtureAndStillAppliesUniverseAndStartChannel()
    {
        var template = GameObject.CreatePrimitive(PrimitiveType.Quad);
        template.name = "FixtureTemplate";
        var primaryReceiver = template.AddComponent<ArtNetReceiver>();
        primaryReceiver.ReceiveNetworkData = false;
        primaryReceiver.DmxBuffer = new DmxBuffer();

        var managerGo = new GameObject("fixture-manager");
        var fixtureMeshManager = managerGo.AddComponent<UI_FixtureMeshManager>();
        SetPrivateField(fixtureMeshManager, "primaryReceiver", primaryReceiver);
        SetPrivateField(fixtureMeshManager, "fixtureTemplate", template);
        fixtureMeshManager.RebuildFixtures(3);

        var bridgeGo = new GameObject("bridge");
        var bridge = bridgeGo.AddComponent<WebUiSettingsBridge>();
        SetPrivateField(bridge, "fixtureMeshManager", fixtureMeshManager);

        bridge.ApplySettings(new WebUiSettingsData
        {
            fixtureMode = "moving",
            dmxUniverse = 9,
            startChannel = 25,
            fixtureAmount = 6,
            gridX = 8,
            gridY = 8
        });

        Assert.That(fixtureMeshManager.FixtureCount, Is.EqualTo(1));
        Assert.That(primaryReceiver.GetUniverseForUserInput(), Is.EqualTo(9));
        Assert.That(primaryReceiver.StartChannel, Is.EqualTo(25));

        Object.DestroyImmediate(bridgeGo);
        Object.DestroyImmediate(managerGo);
        Object.DestroyImmediate(template);
    }





    [Test]
    public void LocalWebUiServer_CachesHtmlFromStreamingAssetsDuringAwake()
    {
        var serverGo = new GameObject("web-server");
        var server = serverGo.AddComponent<LocalWebUiServer>();

        byte[] cachedHtmlBytes = server.GetCachedHtmlBytes();
        string html = System.Text.Encoding.UTF8.GetString(cachedHtmlBytes ?? new byte[0]);

        Assert.That(cachedHtmlBytes, Is.Not.Null);
        Assert.That(cachedHtmlBytes.Length, Is.GreaterThan(0));
        Assert.That(html, Does.Contain("Artnet Fixture Control"));

        Object.DestroyImmediate(serverGo);
    }

    [Test]
    public void LocalWebUiServer_SettingsApi_PostThenGet_RehydratesPersistedPlayerPrefsValues()
    {
        var serverGo = new GameObject("web-server");
        var server = serverGo.AddComponent<LocalWebUiServer>();

        const string postedJson = "{\"deviceName\":\"Desk Node\",\"fixtureMode\":\"surface\",\"dmxUniverse\":7,\"startChannel\":144,\"fixtureAmount\":3,\"gridX\":32,\"gridY\":24}";
        string postResponse = server.HandleSettingsApiRequestImmediately("POST", postedJson);
        WebUiSettingsData postData = WebUiSettingsStore.FromJson(postResponse);

        Assert.That(postData.dmxUniverse, Is.EqualTo(7));
        Assert.That(postData.startChannel, Is.EqualTo(144));
        Assert.That(PlayerPrefs.GetInt("dmx.universe", -1), Is.EqualTo(7));
        Assert.That(PlayerPrefs.GetInt("dmx.channel", -1), Is.EqualTo(144));

        string getResponse = server.HandleSettingsApiRequestImmediately("GET", null);
        WebUiSettingsData getData = WebUiSettingsStore.FromJson(getResponse);

        Assert.That(getData.deviceName, Is.EqualTo("Desk Node"));
        Assert.That(getData.fixtureMode, Is.EqualTo("surface"));
        Assert.That(getData.dmxUniverse, Is.EqualTo(7));
        Assert.That(getData.startChannel, Is.EqualTo(144));

        Object.DestroyImmediate(serverGo);
    }

    [Test]
    public void InAppWebViewSurface_GetWebUiUrl_UsesServerPortAndConfiguredPagePath()
    {
        var serverGo = new GameObject("web-server");
        var server = serverGo.AddComponent<LocalWebUiServer>();
        SetPrivateField(server, "port", 9191);

        var surfaceGo = new GameObject("web-surface");
        var surface = surfaceGo.AddComponent<InAppWebViewSurface>();
        SetPrivateField(surface, "webUiServer", server);
        SetPrivateField(surface, "pagePath", "/index.html?local=true");

        Assert.That(surface.GetWebUiUrl(), Is.EqualTo("http://127.0.0.1:9191/index.html?local=true"));

        Object.DestroyImmediate(surfaceGo);
        Object.DestroyImmediate(serverGo);
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }
}
