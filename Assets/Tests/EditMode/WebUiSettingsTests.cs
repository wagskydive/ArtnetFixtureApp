using System.Reflection;
using System.IO;
using NUnit.Framework;
using UnityEngine;


public class WebUiSettingsTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey("webui.device.name");
        PlayerPrefs.DeleteKey(SaveLoadSettings.WebUiPasswordKey);
        PlayerPrefs.DeleteKey(SaveLoadSettings.WebUiPasswordEnabledKey);
        PlayerPrefs.DeleteKey("dmx.fixture.mode");
        PlayerPrefs.DeleteKey("dmx.universe");
        PlayerPrefs.DeleteKey("dmx.channel");
        PlayerPrefs.DeleteKey("dmx.fixture.count");
        PlayerPrefs.DeleteKey("dmx.pixel.rows");
        PlayerPrefs.DeleteKey("dmx.pixel.columns");
        PlayerPrefs.DeleteKey(SaveLoadSettings.IapEntitlementsKey);
        string customGoboFolder = Path.Combine(Application.persistentDataPath, "CustomGobos");
        if (Directory.Exists(customGoboFolder))
        {
            Directory.Delete(customGoboFolder, true);
        }
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
        Assert.That(loaded.passwordConfigured, Is.False);
    }

    [Test]
    public void ToJson_PreservesResolvedIpAddress()
    {
        var data = new WebUiSettingsData
        {
            deviceName = "Fixture",
            ipAddress = "192.168.1.77",
            fixtureMode = "surface",
            dmxUniverse = 1,
            startChannel = 1,
            fixtureAmount = 1,
            gridX = 8,
            gridY = 8
        };

        string json = WebUiSettingsStore.ToJson(data);
        WebUiSettingsData parsed = WebUiSettingsStore.FromJson(json);

        Assert.That(parsed.ipAddress, Is.EqualTo("192.168.1.77"));
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
        Assert.That(string.IsNullOrWhiteSpace(getData.ipAddress), Is.False);

        Object.DestroyImmediate(serverGo);
    }

    [Test]
    public void LocalWebUiServer_LoginApi_ValidatesConfiguredPassword()
    {
        WebUiPasswordProtection.SetPassword("secret");
        WebUiPasswordProtection.SetEnabled(true);

        var serverGo = new GameObject("web-server");
        var server = serverGo.AddComponent<LocalWebUiServer>();

        string failed = server.HandleLoginApiRequestImmediately("POST", "{\"password\":\"wrong\"}");
        string succeeded = server.HandleLoginApiRequestImmediately("POST", "{\"password\":\"secret\"}");

        Assert.That(failed, Does.Contain("\"authenticated\":false"));
        Assert.That(succeeded, Does.Contain("\"authenticated\":true"));

        Object.DestroyImmediate(serverGo);
    }

    [Test]
    public void LocalWebUiServer_ImagesApi_Returns16SlotsAndLockedStateWhenIapMissing()
    {
        var serverGo = new GameObject("web-server");
        var server = serverGo.AddComponent<LocalWebUiServer>();

        string response = server.HandleImagesApiRequestImmediately("GET");

        Assert.That(response, Does.Contain("\"unlocked\":false"));
        Assert.That(response, Does.Contain("\"slot\":1"));
        Assert.That(response, Does.Contain("\"slot\":16"));

        Object.DestroyImmediate(serverGo);
    }

    [Test]
    public void CustomGoboStorage_TrySaveSlotPng_RejectsInvalidSlotAndSize()
    {
        byte[] tinyPng = CreatePng(128, 128);

        bool invalidSlotSaved = CustomGoboStorage.TrySaveSlotPng(0, tinyPng, out string slotError);
        bool wrongSizeSaved = CustomGoboStorage.TrySaveSlotPng(1, tinyPng, out string sizeError);

        Assert.That(invalidSlotSaved, Is.False);
        Assert.That(slotError, Does.Contain("Slot must be between"));
        Assert.That(wrongSizeSaved, Is.False);
        Assert.That(sizeError, Does.Contain("512x512"));
    }

    [Test]
    public void CustomGoboStorage_TrySaveSlotPng_AcceptsValidRgba512Png()
    {
        byte[] validPng = CreatePng(512, 512);

        bool saved = CustomGoboStorage.TrySaveSlotPng(2, validPng, out string error);
        string path = CustomGoboStorage.GetSlotPath(2);

        Assert.That(saved, Is.True);
        Assert.That(error, Is.Null.Or.Empty);
        Assert.That(File.Exists(path), Is.True);
    }

    [Test]
    public void CustomGoboStorage_TryDeleteSlotAndCompact_ReordersRemainingSlotsWithoutGaps()
    {
        byte[] validPng = CreatePng(512, 512);
        Assert.That(CustomGoboStorage.TrySaveSlotPng(1, validPng, out _), Is.True);
        Assert.That(CustomGoboStorage.TrySaveSlotPng(2, validPng, out _), Is.True);
        Assert.That(CustomGoboStorage.TrySaveSlotPng(3, validPng, out _), Is.True);

        bool removed = CustomGoboStorage.TryDeleteSlotAndCompact(2, out string error);

        Assert.That(removed, Is.True);
        Assert.That(error, Is.Null.Or.Empty);
        Assert.That(File.Exists(CustomGoboStorage.GetSlotPath(1)), Is.True);
        Assert.That(File.Exists(CustomGoboStorage.GetSlotPath(2)), Is.True);
        Assert.That(File.Exists(CustomGoboStorage.GetSlotPath(3)), Is.False);
    }



    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(target, value);
    }

    private static byte[] CreatePng(int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.5f));
        texture.Apply();
        byte[] png = texture.EncodeToPNG();
        Object.DestroyImmediate(texture);
        return png;
    }
}
